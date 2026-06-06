using System.Collections.ObjectModel;

namespace SevexLabs.Ui.Maui.Controls
{
    public enum PopCountdownStartValueMode
    {
        Default,
        StartScaleAnimated,
        EndScaleStatic
    }

    /// <summary>
    /// Countdown view that animates each displayed value with a pop-scale text effect.
    /// </summary>
    /// <remarks>
    /// Running tasks and animations are cleaned up silently on unload or handler disconnect.
    /// </remarks>
    public class PopCountdownView : ContentView
    {
        private const string ScaleAnimationName = "PopCountdownScaleAnimation";

        private static readonly Easing LateSlowdownEasing = new(t =>
        {
            const double slowStart = 0.70d;

            if (t <= 0d)
            {
                return 0d;
            }

            if (t >= 1d)
            {
                return 1d;
            }

            if (t < slowStart)
            {
                double normalized = t / slowStart;
                return normalized * slowStart;
            }

            double tailT = (t - slowStart) / (1d - slowStart);
            double cubicOut = 1d - Math.Pow(1d - tailT, 3d);

            return slowStart + ((1d - slowStart) * cubicOut);
        });

        private enum CountdownState
        {
            Idle,
            Running,
            Pausing,
            Paused,
            Completed
        }

        private sealed class CountdownStep
        {
            public CountdownStep(string displayText, int? numericValue)
            {
                DisplayText = displayText;
                NumericValue = numericValue;
            }

            public string DisplayText { get; }

            public int? NumericValue { get; }
        }

        private sealed class CountdownDrawable : IDrawable
        {
            private readonly PopCountdownView _owner;

            public CountdownDrawable(PopCountdownView owner)
            {
                _owner = owner;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                if (string.IsNullOrWhiteSpace(_owner.CurrentDisplayText))
                {
                    return;
                }

                _owner.DrawDepth(canvas, dirtyRect, _owner.CurrentDisplayText);
                _owner.DrawMainText(canvas, dirtyRect, _owner.CurrentDisplayText);
            }
        }

        private readonly SemaphoreSlim _runLock = new(1, 1);

        private readonly Grid _root;
        private readonly BoxView _solidBackgroundView;
        private readonly GraphicsView _textLayer;

        private CancellationTokenSource? _runCts;
        private CountdownState _state = CountdownState.Idle;

        private bool _isInternalValueChange;
        private bool _suppressIsRunningCallback;
        private bool _skipInitialStartValuePresentation;

        private string _currentDisplayText = string.Empty;
        private List<CountdownStep> _steps = new();
        private int _nextStepIndex;

        private double _currentTextScale = 1d;

        public PopCountdownView()
        {
            base.BackgroundColor = Colors.Transparent;

            _solidBackgroundView = new BoxView
            {
                IsVisible = false,
                InputTransparent = true,
                Color = Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _textLayer = new GraphicsView
            {
                Drawable = new CountdownDrawable(this),
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                AnchorX = 0d,
                AnchorY = 0d,
                Scale = 1d,
                TranslationX = 0d,
                TranslationY = 0d,
                Opacity = 0d
            };

            _textLayer.SizeChanged += OnTextLayerSizeChanged;
            Loaded += OnLoaded;
            Unloaded += HandleUnloaded;

            _root = new Grid
            {
                InputTransparent = true,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _root.Children.Add(_solidBackgroundView);
            _root.Children.Add(_textLayer);

            base.Content = _root;

            if (FinalTexts is null)
            {
                FinalTexts = new ObservableCollection<string>();
            }

            _steps = BuildSequence();

            SetValueInternal(StartValue, raiseEvent: false);
            UpdateDisplayText(StartValue.ToString(), raiseEvent: false);
            ApplyInitialScaleState();

            UpdateBackgroundVisualState();
        }

        #region Bindable Properties

        public static readonly BindableProperty StartValueProperty =
            BindableProperty.Create(
                nameof(StartValue),
                typeof(int),
                typeof(PopCountdownView),
                5,
                propertyChanged: OnStartOrEndValueChanged);

        public static readonly BindableProperty EndValueProperty =
            BindableProperty.Create(
                nameof(EndValue),
                typeof(int),
                typeof(PopCountdownView),
                0,
                propertyChanged: OnStartOrEndValueChanged);

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(
                nameof(Value),
                typeof(int),
                typeof(PopCountdownView),
                5,
                propertyChanged: OnValuePropertyChanged);

        public static readonly BindableProperty IntervalProperty =
            BindableProperty.Create(
                nameof(Interval),
                typeof(TimeSpan),
                typeof(PopCountdownView),
                TimeSpan.FromSeconds(1));

        public static readonly BindableProperty IsRunningProperty =
            BindableProperty.Create(
                nameof(IsRunning),
                typeof(bool),
                typeof(PopCountdownView),
                false,
                propertyChanged: OnIsRunningPropertyChanged);

        public static readonly BindableProperty FinalTextsProperty =
            BindableProperty.Create(
                nameof(FinalTexts),
                typeof(IList<string>),
                typeof(PopCountdownView),
                defaultValueCreator: _ => new ObservableCollection<string>(),
                propertyChanged: OnFinalTextsPropertyChanged);

        public static readonly BindableProperty AnimateOnValueChangedProperty =
            BindableProperty.Create(
                nameof(AnimateOnValueChanged),
                typeof(bool),
                typeof(PopCountdownView),
                false);

        public static readonly BindableProperty HideOnCompletedProperty =
            BindableProperty.Create(
                nameof(HideOnCompleted),
                typeof(bool),
                typeof(PopCountdownView),
                false);

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(
                nameof(TextColor),
                typeof(Color),
                typeof(PopCountdownView),
                Colors.White,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create(
                nameof(FontFamily),
                typeof(string),
                typeof(PopCountdownView),
                default(string),
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(
                nameof(FontSize),
                typeof(double),
                typeof(PopCountdownView),
                120d,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty DepthColorProperty =
            BindableProperty.Create(
                nameof(DepthColor),
                typeof(Color),
                typeof(PopCountdownView),
                Colors.Black,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty DepthOffsetXProperty =
            BindableProperty.Create(
                nameof(DepthOffsetX),
                typeof(double),
                typeof(PopCountdownView),
                10d,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty DepthOffsetYProperty =
            BindableProperty.Create(
                nameof(DepthOffsetY),
                typeof(double),
                typeof(PopCountdownView),
                10d,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty DepthLayersProperty =
            BindableProperty.Create(
                nameof(DepthLayers),
                typeof(int),
                typeof(PopCountdownView),
                3,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty UseSingleDepthLayerProperty =
            BindableProperty.Create(
                nameof(UseSingleDepthLayer),
                typeof(bool),
                typeof(PopCountdownView),
                true,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty IsShadowEnabledProperty =
            BindableProperty.Create(
                nameof(IsShadowEnabled),
                typeof(bool),
                typeof(PopCountdownView),
                false,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create(
                nameof(ShadowColor),
                typeof(Color),
                typeof(PopCountdownView),
                Color.FromArgb("#99000000"),
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty ShadowOffsetXProperty =
            BindableProperty.Create(
                nameof(ShadowOffsetX),
                typeof(double),
                typeof(PopCountdownView),
                0d,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty ShadowOffsetYProperty =
            BindableProperty.Create(
                nameof(ShadowOffsetY),
                typeof(double),
                typeof(PopCountdownView),
                8d,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty ShadowBlurProperty =
            BindableProperty.Create(
                nameof(ShadowBlur),
                typeof(float),
                typeof(PopCountdownView),
                12f,
                propertyChanged: OnTextVisualPropertyChanged);

        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create(
                nameof(AnimationDuration),
                typeof(TimeSpan),
                typeof(PopCountdownView),
                TimeSpan.FromMilliseconds(180));

        public static readonly BindableProperty StartScaleProperty =
            BindableProperty.Create(
                nameof(StartScale),
                typeof(double),
                typeof(PopCountdownView),
                0.05d,
                propertyChanged: OnInitialScaleConfigurationChanged);

        public static readonly BindableProperty EndScaleProperty =
            BindableProperty.Create(
                nameof(EndScale),
                typeof(double),
                typeof(PopCountdownView),
                1.75d,
                propertyChanged: OnInitialScaleConfigurationChanged);

        public static readonly BindableProperty StartValueModeProperty =
            BindableProperty.Create(
                nameof(StartValueMode),
                typeof(PopCountdownStartValueMode),
                typeof(PopCountdownView),
                PopCountdownStartValueMode.Default,
                propertyChanged: OnInitialScaleConfigurationChanged);

        public static readonly new BindableProperty BackgroundColorProperty =
            BindableProperty.Create(
                nameof(BackgroundColor),
                typeof(Color),
                typeof(PopCountdownView),
                Colors.Transparent,
                propertyChanged: OnBackgroundVisualPropertyChanged);

        #endregion

        #region Public Properties

        public int StartValue
        {
            get => (int)GetValue(StartValueProperty);
            set => SetValue(StartValueProperty, value);
        }

        public int EndValue
        {
            get => (int)GetValue(EndValueProperty);
            set => SetValue(EndValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the current numeric value displayed by the countdown sequence.
        /// </summary>
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public TimeSpan Interval
        {
            get => (TimeSpan)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the countdown is running.
        /// </summary>
        /// <remarks>
        /// Setting this property starts or stops the countdown through the same public lifecycle path.
        /// </remarks>
        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        /// <summary>
        /// Gets or sets optional non-numeric texts displayed after the numeric countdown reaches the end value.
        /// </summary>
        public IList<string> FinalTexts
        {
            get => (IList<string>)GetValue(FinalTextsProperty);
            set => SetValue(FinalTextsProperty, value);
        }

        public bool AnimateOnValueChanged
        {
            get => (bool)GetValue(AnimateOnValueChangedProperty);
            set => SetValue(AnimateOnValueChangedProperty, value);
        }

        public bool HideOnCompleted
        {
            get => (bool)GetValue(HideOnCompletedProperty);
            set => SetValue(HideOnCompletedProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public string? FontFamily
        {
            get => (string?)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public Color DepthColor
        {
            get => (Color)GetValue(DepthColorProperty);
            set => SetValue(DepthColorProperty, value);
        }

        public double DepthOffsetX
        {
            get => (double)GetValue(DepthOffsetXProperty);
            set => SetValue(DepthOffsetXProperty, value);
        }

        public double DepthOffsetY
        {
            get => (double)GetValue(DepthOffsetYProperty);
            set => SetValue(DepthOffsetYProperty, value);
        }

        public int DepthLayers
        {
            get => (int)GetValue(DepthLayersProperty);
            set => SetValue(DepthLayersProperty, value);
        }

        public bool UseSingleDepthLayer
        {
            get => (bool)GetValue(UseSingleDepthLayerProperty);
            set => SetValue(UseSingleDepthLayerProperty, value);
        }

        public bool IsShadowEnabled
        {
            get => (bool)GetValue(IsShadowEnabledProperty);
            set => SetValue(IsShadowEnabledProperty, value);
        }

        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public double ShadowOffsetX
        {
            get => (double)GetValue(ShadowOffsetXProperty);
            set => SetValue(ShadowOffsetXProperty, value);
        }

        public double ShadowOffsetY
        {
            get => (double)GetValue(ShadowOffsetYProperty);
            set => SetValue(ShadowOffsetYProperty, value);
        }

        public float ShadowBlur
        {
            get => (float)GetValue(ShadowBlurProperty);
            set => SetValue(ShadowBlurProperty, value);
        }

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public double StartScale
        {
            get => (double)GetValue(StartScaleProperty);
            set => SetValue(StartScaleProperty, value);
        }

        public double EndScale
        {
            get => (double)GetValue(EndScaleProperty);
            set => SetValue(EndScaleProperty, value);
        }

        public PopCountdownStartValueMode StartValueMode
        {
            get => (PopCountdownStartValueMode)GetValue(StartValueModeProperty);
            set => SetValue(StartValueModeProperty, value);
        }

        public new Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public string CurrentDisplayText => _currentDisplayText;

        public new View Content
        {
            get => base.Content;
            private set => base.Content = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a countdown run starts.
        /// </summary>
        public event EventHandler? Started;

        /// <summary>
        /// Raised when a running countdown is paused.
        /// </summary>
        public event EventHandler? Paused;

        /// <summary>
        /// Raised when the countdown is explicitly stopped.
        /// </summary>
        public event EventHandler? Stopped;

        /// <summary>
        /// Raised when the countdown sequence completes, including any final texts.
        /// </summary>
        public event EventHandler? Completed;

        /// <summary>
        /// Raised when the numeric countdown value changes.
        /// </summary>
        public event EventHandler<PopCountdownValueChangedEventArgs>? ValueChanged;

        /// <summary>
        /// Raised when the displayed text changes, including final text entries.
        /// </summary>
        public event EventHandler<PopCountdownDisplayTextChangedEventArgs>? DisplayTextChanged;

        #endregion

        #region Drawing Helpers

        internal void DrawDepth(ICanvas canvas, RectF rect, string text)
        {
            int layers = Math.Max(0, DepthLayers);
            if (layers <= 0)
            {
                return;
            }

            if (UseSingleDepthLayer || layers == 1)
            {
                canvas.SaveState();
                canvas.Translate((float)DepthOffsetX, (float)DepthOffsetY);

                ApplyTextStyle(canvas, DepthColor);
                canvas.DrawString(
                    text,
                    rect,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center,
                    TextFlow.OverflowBounds);

                canvas.RestoreState();
                return;
            }

            float stepX = (float)(DepthOffsetX / layers);
            float stepY = (float)(DepthOffsetY / layers);

            for (int i = layers; i >= 1; i--)
            {
                canvas.SaveState();
                canvas.Translate(stepX * i, stepY * i);

                ApplyTextStyle(canvas, DepthColor);
                canvas.DrawString(
                    text,
                    rect,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center,
                    TextFlow.OverflowBounds);

                canvas.RestoreState();
            }
        }

        internal void DrawMainText(ICanvas canvas, RectF rect, string text)
        {
            canvas.SaveState();

            ApplyShadow(canvas);
            ApplyTextStyle(canvas, TextColor);

            canvas.DrawString(
                text,
                rect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center,
                TextFlow.OverflowBounds);

            ClearShadow(canvas);
            canvas.RestoreState();
        }

        private void ApplyTextStyle(ICanvas canvas, Color color)
        {
            canvas.FontColor = color;
            canvas.FontSize = (float)FontSize;

            if (!string.IsNullOrWhiteSpace(FontFamily))
            {
                canvas.Font = new Microsoft.Maui.Graphics.Font(FontFamily);
            }
            else
            {
                canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            }
        }

        private void ApplyShadow(ICanvas canvas)
        {
            if (!IsShadowEnabled || ShadowColor.Alpha <= 0f)
            {
                ClearShadow(canvas);
                return;
            }

            canvas.SetShadow(
                new SizeF((float)ShadowOffsetX, (float)ShadowOffsetY),
                ShadowBlur,
                ShadowColor);
        }

        private static void ClearShadow(ICanvas canvas)
        {
            canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);
        }

        private void InvalidateTextLayer()
        {
            _textLayer.Invalidate();
        }

        private void UpdateBackgroundVisualState()
        {
            _solidBackgroundView.BackgroundColor = Colors.Transparent;
            _solidBackgroundView.Color = BackgroundColor;
            _solidBackgroundView.IsVisible = BackgroundColor.Alpha > 0f;
        }

        private double GetInitialScale()
        {
            return StartValueMode switch
            {
                PopCountdownStartValueMode.StartScaleAnimated => StartScale,
                PopCountdownStartValueMode.EndScaleStatic => EndScale,
                _ => 1d
            };
        }

        private void SetCurrentTextScale(double scale)
        {
            _currentTextScale = Math.Max(0.0001d, scale);
            ApplyTextLayerTransform();
        }

        private void ApplyInitialScaleState()
        {
            SetCurrentTextScale(GetInitialScale());
        }

        private bool ShouldSkipInitialStartValueAnimation()
        {
            return StartValueMode == PopCountdownStartValueMode.EndScaleStatic;
        }

        private void ApplyTextLayerTransform()
        {
            double width = _textLayer.Width;
            double height = _textLayer.Height;
            double scale = _currentTextScale;

            if (width <= 0d || height <= 0d)
            {
                _textLayer.Scale = 1d;
                _textLayer.TranslationX = 0d;
                _textLayer.TranslationY = 0d;
                _textLayer.Opacity = 0d;
                return;
            }

            _textLayer.Scale = scale;
            _textLayer.TranslationX = width * (1d - scale) / 2d;
            _textLayer.TranslationY = height * (1d - scale) / 2d;
            _textLayer.Opacity = 1d;
        }

        private void OnTextLayerSizeChanged(object? sender, EventArgs e)
        {
            ApplyTextLayerTransform();
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            ApplyTextLayerTransform();
        }

        private void HandleUnloaded(object? sender, EventArgs e)
        {
            CleanupLifecycleResources();
        }

        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            if (args.OldHandler is not null)
            {
                CleanupLifecycleResources();
            }

            base.OnHandlerChanging(args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts a new run or resumes a paused countdown.
        /// </summary>
        public async Task StartAsync()
        {
            await _runLock.WaitAsync();

            try
            {
                if (StartValue < EndValue)
                {
                    throw new InvalidOperationException(
                        $"{nameof(StartValue)} must be greater than or equal to {nameof(EndValue)}.");
                }

                if (_state is CountdownState.Running or CountdownState.Pausing)
                {
                    return;
                }

                bool isResume = _state == CountdownState.Paused;

                IsVisible = true;

                if (!isResume)
                {
                    PrepareNewRunFromBeginning();
                }

                _runCts?.Cancel();
                _runCts?.Dispose();
                var runCts = new CancellationTokenSource();
                _runCts = runCts;

                EnsureRunningFlag(true);
                _state = CountdownState.Running;
                Started?.Invoke(this, EventArgs.Empty);

                _ = RunCountdownLoopAsync(runCts);
            }
            finally
            {
                _runLock.Release();
            }
        }

        /// <summary>
        /// Requests a pause of the running countdown.
        /// </summary>
        public void Pause()
        {
            if (_state == CountdownState.Running)
            {
                _state = CountdownState.Pausing;
            }
        }

        /// <summary>
        /// Stops the countdown, resets it to the initial state, and raises <see cref="Stopped"/>.
        /// </summary>
        public void Stop()
        {
            HardStop(resetToInitialState: true, raiseStoppedEvent: true);
        }

        /// <summary>
        /// Stops the countdown and resets it without raising <see cref="Stopped"/>.
        /// </summary>
        public void Reset()
        {
            HardStop(resetToInitialState: true, raiseStoppedEvent: false);
        }

        /// <summary>
        /// Resets the countdown and starts it again.
        /// </summary>
        public async Task RestartAsync()
        {
            HardStop(resetToInitialState: true, raiseStoppedEvent: false);
            await StartAsync();
        }

        #endregion

        #region Core Logic

        private async Task RunCountdownLoopAsync(CancellationTokenSource runCts)
        {
            var token = runCts.Token;

            try
            {
                while (_nextStepIndex < _steps.Count)
                {
                    token.ThrowIfCancellationRequested();

                    if (_skipInitialStartValuePresentation && _nextStepIndex == 0)
                    {
                        if (Interval > TimeSpan.Zero)
                        {
                            await Task.Delay(Interval, token);
                        }

                        if (_state == CountdownState.Pausing)
                        {
                            _state = CountdownState.Paused;
                            EnsureRunningFlag(false);
                            Paused?.Invoke(this, EventArgs.Empty);
                            return;
                        }

                        _skipInitialStartValuePresentation = false;
                        _nextStepIndex++;
                        continue;
                    }

                    CountdownStep step = _steps[_nextStepIndex];

                    if (step.NumericValue.HasValue)
                    {
                        SetValueInternal(step.NumericValue.Value, raiseEvent: true);
                    }

                    UpdateDisplayText(step.DisplayText, raiseEvent: true);

                    await PlayCurrentStepAnimationAsync(token);

                    _nextStepIndex++;

                    if (_state == CountdownState.Pausing)
                    {
                        _state = CountdownState.Paused;
                        EnsureRunningFlag(false);
                        Paused?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    if (_nextStepIndex < _steps.Count)
                    {
                        TimeSpan remaining = Interval - AnimationDuration;
                        if (remaining > TimeSpan.Zero)
                        {
                            await Task.Delay(remaining, token);
                        }
                    }
                }

                _state = CountdownState.Completed;
                EnsureRunningFlag(false);
                Completed?.Invoke(this, EventArgs.Empty);

                if (HideOnCompleted)
                {
                    IsVisible = false;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _skipInitialStartValuePresentation = false;

                if (ReferenceEquals(_runCts, runCts))
                {
                    _runCts.Dispose();
                    _runCts = null;
                }

                if (_state == CountdownState.Running)
                {
                    _state = CountdownState.Idle;
                    EnsureRunningFlag(false);
                }
            }
        }

        private List<CountdownStep> BuildSequence()
        {
            var result = new List<CountdownStep>();

            for (int i = StartValue; i >= EndValue; i--)
            {
                result.Add(new CountdownStep(i.ToString(), i));
            }

            if (FinalTexts is not null)
            {
                foreach (string text in FinalTexts)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Add(new CountdownStep(text, null));
                    }
                }
            }

            return result;
        }

        private void PrepareNewRunFromBeginning()
        {
            _textLayer.AbortAnimation(ScaleAnimationName);

            _steps = BuildSequence();
            _nextStepIndex = 0;
            _state = CountdownState.Idle;
            _skipInitialStartValuePresentation = ShouldSkipInitialStartValueAnimation();

            SetValueInternal(StartValue, raiseEvent: false);
            UpdateDisplayText(StartValue.ToString(), raiseEvent: false);
            ApplyInitialScaleState();
        }

        private async Task PlayCurrentStepAnimationAsync(CancellationToken token)
        {
            uint length = (uint)Math.Max(1, AnimationDuration.TotalMilliseconds);

            double start = StartScale;
            double end = EndScale;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _textLayer.AbortAnimation(ScaleAnimationName);
                SetCurrentTextScale(start);
            });

            await Task.Yield();
            token.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var animation = new Animation(
                    callback: value => SetCurrentTextScale(value),
                    start: start,
                    end: end,
                    easing: Easing.CubicOut);
                // easing: LateSlowdownEasing);

                animation.Commit(
                    owner: _textLayer,
                    name: ScaleAnimationName,
                    rate: 16,
                    length: length,
                    finished: (_, _) =>
                    {
                        SetCurrentTextScale(end);
                        tcs.TrySetResult(true);
                    });
            });

            using (token.Register(() =>
                   {
                       MainThread.BeginInvokeOnMainThread(() => _textLayer.AbortAnimation(ScaleAnimationName));
                       tcs.TrySetCanceled(token);
                   }))
            {
                await tcs.Task;
            }
        }

        private void CleanupLifecycleResources()
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = null;

            _skipInitialStartValuePresentation = false;
            _state = CountdownState.Idle;

            _textLayer.AbortAnimation(ScaleAnimationName);
            EnsureRunningFlag(false);
        }

        private void HardStop(bool resetToInitialState, bool raiseStoppedEvent)
        {
            bool wasBusy =
                _state == CountdownState.Running ||
                _state == CountdownState.Pausing ||
                _state == CountdownState.Paused;

            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = null;
            _skipInitialStartValuePresentation = false;

            _textLayer.AbortAnimation(ScaleAnimationName);
            EnsureRunningFlag(false);

            if (resetToInitialState)
            {
                _state = CountdownState.Idle;
                _steps = BuildSequence();
                _nextStepIndex = 0;

                IsVisible = true;

                SetValueInternal(StartValue, raiseEvent: true);
                UpdateDisplayText(StartValue.ToString(), raiseEvent: true);
                ApplyInitialScaleState();
            }

            if (raiseStoppedEvent && wasBusy)
            {
                Stopped?.Invoke(this, EventArgs.Empty);
            }
        }

        private void EnsureRunningFlag(bool value)
        {
            if (IsRunning == value)
            {
                return;
            }

            _suppressIsRunningCallback = true;
            try
            {
                SetValue(IsRunningProperty, value);
            }
            finally
            {
                _suppressIsRunningCallback = false;
            }
        }

        private void SetValueInternal(int newValue, bool raiseEvent)
        {
            int oldValue = Value;

            if (oldValue == newValue)
            {
                return;
            }

            _isInternalValueChange = true;
            try
            {
                SetValue(ValueProperty, newValue);
            }
            finally
            {
                _isInternalValueChange = false;
            }

            if (raiseEvent)
            {
                ValueChanged?.Invoke(this, new PopCountdownValueChangedEventArgs(oldValue, newValue));
            }
        }

        private void UpdateDisplayText(string newText, bool raiseEvent)
        {
            if (_currentDisplayText == newText)
            {
                InvalidateTextLayer();
                return;
            }

            string oldText = _currentDisplayText;
            _currentDisplayText = newText;

            if (raiseEvent)
            {
                DisplayTextChanged?.Invoke(
                    this,
                    new PopCountdownDisplayTextChangedEventArgs(oldText, newText));
            }

            InvalidateTextLayer();
        }

        #endregion

        #region Property Changed Callbacks

        private static void OnTextVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is PopCountdownView view)
            {
                view.InvalidateTextLayer();
            }
        }

        private static void OnBackgroundVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is PopCountdownView view)
            {
                view.UpdateBackgroundVisualState();
            }
        }

        private static void OnInitialScaleConfigurationChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not PopCountdownView view)
            {
                return;
            }

            if (view._state is CountdownState.Running or CountdownState.Pausing)
            {
                return;
            }

            view.ApplyInitialScaleState();
        }

        private static void OnStartOrEndValueChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not PopCountdownView view)
            {
                return;
            }

            if (view._state is CountdownState.Running or CountdownState.Pausing)
            {
                return;
            }

            view._steps = view.BuildSequence();
            view._nextStepIndex = 0;

            if (view._state is CountdownState.Idle or CountdownState.Paused or CountdownState.Completed)
            {
                view.SetValueInternal(view.StartValue, raiseEvent: false);
                view.UpdateDisplayText(view.StartValue.ToString(), raiseEvent: false);
                view.ApplyInitialScaleState();
            }
        }

        private static void OnFinalTextsPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not PopCountdownView view)
            {
                return;
            }

            if (view._state is CountdownState.Running or CountdownState.Pausing)
            {
                return;
            }

            view._steps = view.BuildSequence();
            view._nextStepIndex = 0;
            view.InvalidateTextLayer();
        }

        private static void OnIsRunningPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not PopCountdownView view || view._suppressIsRunningCallback)
            {
                return;
            }

            bool isRunning = (bool)newValue!;

            if (isRunning)
            {
                _ = view.StartAsync();
            }
            else
            {
                view.Stop();
            }
        }

        private static void OnValuePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not PopCountdownView view)
            {
                return;
            }

            int oldInt = (int)oldValue!;
            int newInt = (int)newValue!;

            if (oldInt == newInt)
            {
                return;
            }

            if (!view._isInternalValueChange)
            {
                view.ValueChanged?.Invoke(
                    view,
                    new PopCountdownValueChangedEventArgs(oldInt, newInt));

                view.UpdateDisplayText(newInt.ToString(), raiseEvent: true);

                if (view.AnimateOnValueChanged)
                {
                    _ = view.PlayCurrentStepAnimationAsync(CancellationToken.None);
                }
                else
                {
                    view.SetCurrentTextScale(view.EndScale);
                }
            }
        }

        #endregion
    }
}
