using System.Globalization;
using SevexLabs.Ui.Maui.Controls.Enum;

namespace SevexLabs.Ui.Maui.Controls
{
    /// <summary>
    /// Base graphics countdown control that owns countdown state, timer behavior, and lifecycle cleanup.
    /// </summary>
    /// <remarks>
    /// Timers and animations are cleaned up silently on unload or handler disconnect.
    /// </remarks>
    public abstract class CountdownViewBase : GraphicsView
    {
        private const string AnimationName = "CountdownAnimation";
        private const double Epsilon = 0.0000001d;

        private IDispatcherTimer? _timer;
        private double _currentValue;
        private double _progress;
        private double _displayedValue;
        private double _displayedProgress;
        private bool _isValueInitialized;

        protected CountdownViewBase()
        {
            BackgroundColor = Colors.Transparent;
            Unloaded += HandleUnloaded;
        }

        #region BindableProperties

        public static readonly BindableProperty StartValueProperty =
            BindableProperty.Create(
                nameof(StartValue),
                typeof(double),
                typeof(CountdownViewBase),
                0d,
                propertyChanged: OnBehaviorPropertyChanged);

        public static readonly BindableProperty FinalValueProperty =
            BindableProperty.Create(
                nameof(FinalValue),
                typeof(double),
                typeof(CountdownViewBase),
                0d,
                propertyChanged: OnBehaviorPropertyChanged);

        public static readonly BindableProperty StepValueProperty =
            BindableProperty.Create(
                nameof(StepValue),
                typeof(double),
                typeof(CountdownViewBase),
                1d,
                propertyChanged: OnBehaviorPropertyChanged);

        public static readonly BindableProperty StepIntervalProperty =
            BindableProperty.Create(
                nameof(StepInterval),
                typeof(TimeSpan),
                typeof(CountdownViewBase),
                TimeSpan.FromSeconds(1),
                propertyChanged: OnBehaviorPropertyChanged);

        public static readonly BindableProperty MaxDecimalsProperty =
            BindableProperty.Create(
                nameof(MaxDecimals),
                typeof(int),
                typeof(CountdownViewBase),
                1,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty DirectionProperty =
            BindableProperty.Create(
                nameof(Direction),
                typeof(CountdownDirection),
                typeof(CountdownViewBase),
                CountdownDirection.Clockwise,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty AnimateProperty =
            BindableProperty.Create(
                nameof(Animate),
                typeof(bool),
                typeof(CountdownViewBase),
                false,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create(
                nameof(AnimationDuration),
                typeof(uint),
                typeof(CountdownViewBase),
                (uint)300,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create(
                nameof(FontFamily),
                typeof(string),
                typeof(CountdownViewBase),
                default(string),
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(
                nameof(FontSize),
                typeof(double),
                typeof(CountdownViewBase),
                24d,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(
                nameof(TextColor),
                typeof(Color),
                typeof(CountdownViewBase),
                Colors.Black,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty TextFormatProperty =
            BindableProperty.Create(
                nameof(TextFormat),
                typeof(string),
                typeof(CountdownViewBase),
                default(string),
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty TimeDisplayFormatProperty =
            BindableProperty.Create(
                nameof(TimeDisplayFormat),
                typeof(CountdownTimeDisplayFormat),
                typeof(CountdownViewBase),
                CountdownTimeDisplayFormat.Ss,
                propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty ValueSuffixProperty =
            BindableProperty.Create(
                nameof(ValueSuffix),
                typeof(string),
                typeof(CountdownViewBase),
                string.Empty,
                propertyChanged: OnVisualPropertyChanged);

        #endregion

        #region Public Properties

        public double StartValue
        {
            get => (double)GetValue(StartValueProperty);
            set => SetValue(StartValueProperty, value);
        }

        public double FinalValue
        {
            get => (double)GetValue(FinalValueProperty);
            set => SetValue(FinalValueProperty, value);
        }

        public double StepValue
        {
            get => (double)GetValue(StepValueProperty);
            set => SetValue(StepValueProperty, value);
        }

        public TimeSpan StepInterval
        {
            get => (TimeSpan)GetValue(StepIntervalProperty);
            set => SetValue(StepIntervalProperty, value);
        }

        public int MaxDecimals
        {
            get => (int)GetValue(MaxDecimalsProperty);
            set => SetValue(MaxDecimalsProperty, value);
        }

        public CountdownDirection Direction
        {
            get => (CountdownDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public bool Animate
        {
            get => (bool)GetValue(AnimateProperty);
            set => SetValue(AnimateProperty, value);
        }

        public uint AnimationDuration
        {
            get => (uint)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
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

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        /// <summary>
        /// Optional numeric string format used when TimeDisplayFormat is Ss.
        /// Examples:
        /// "{0}"
        /// "{0:F0}"
        /// "{0:F1}"
        /// "{0:0.##}"
        /// </summary>
        public string? TextFormat
        {
            get => (string?)GetValue(TextFormatProperty);
            set => SetValue(TextFormatProperty, value);
        }

        /// <summary>
        /// Controls how the countdown value is displayed.
        /// The source value is always interpreted as seconds.
        /// </summary>
        public CountdownTimeDisplayFormat TimeDisplayFormat
        {
            get => (CountdownTimeDisplayFormat)GetValue(TimeDisplayFormatProperty);
            set => SetValue(TimeDisplayFormatProperty, value);
        }

        public string ValueSuffix
        {
            get => (string)GetValue(ValueSuffixProperty);
            set => SetValue(ValueSuffixProperty, value);
        }

        public double CurrentValue
        {
            get => _currentValue;
            protected set
            {
                if (AreClose(_currentValue, value))
                    return;

                _currentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
            }
        }

        /// <summary>
        /// Progress normalized between 0 and 1.
        /// 1 means full, 0 means completed.
        /// </summary>
        public double Progress
        {
            get => _progress;
            protected set
            {
                var normalized = Clamp01(value);

                if (AreClose(_progress, normalized))
                    return;

                _progress = normalized;
                OnPropertyChanged(nameof(Progress));
            }
        }

        /// <summary>
        /// Gets the current countdown state.
        /// </summary>
        public CountdownState State { get; private set; } = CountdownState.Stopped;

        /// <summary>
        /// Gets whether the countdown is currently running.
        /// </summary>
        public bool IsRunning => State == CountdownState.Running;

        /// <summary>
        /// Gets whether the countdown is paused.
        /// </summary>
        public bool IsPaused => State == CountdownState.Paused;

        /// <summary>
        /// Gets whether the countdown has reached its final value.
        /// </summary>
        public bool IsCompleted => State == CountdownState.Completed;

        #endregion

        #region Events

        /// <summary>
        /// Raised when a countdown run starts.
        /// </summary>
        public event EventHandler? CountdownStarted;

        /// <summary>
        /// Raised when a running countdown is paused.
        /// </summary>
        public event EventHandler? CountdownPaused;

        /// <summary>
        /// Raised when the countdown is explicitly stopped and reset to the start value.
        /// </summary>
        public event EventHandler? CountdownStopped;

        /// <summary>
        /// Raised after each countdown step updates the current value and progress.
        /// </summary>
        public event EventHandler<CountdownUpdatedEventArgs>? CountdownUpdated;

        /// <summary>
        /// Raised when the countdown reaches its final value.
        /// </summary>
        public event EventHandler<CountdownCompletedEventArgs>? CountdownCompleted;

        #endregion

        #region Internal Rendering State

        internal double DisplayedValue => _displayedValue;
        internal double DisplayedProgress => _displayedProgress;

        #endregion

        #region Lifecycle

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is not null)
            {
                EnsureTimer();
            }

            if (!_isValueInitialized && StartValue > 0)
            {
                ResetToStartValue();
                SetState(CountdownState.Stopped);
            }
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
        /// Starts a new countdown or resumes a paused countdown.
        /// </summary>
        /// <remarks>
        /// Starting from the stopped state resets the value to <see cref="StartValue"/> before running.
        /// </remarks>
        public void Start()
        {
            ValidateConfiguration();
            EnsureTimer();

            if (State == CountdownState.Running)
                return;

            if (!_isValueInitialized || State == CountdownState.Stopped)
            {
                ResetToStartValue();
            }

            OnCountdownStarted();

            if (AreClose(StartValue, FinalValue) || AreClose(CurrentValue, FinalValue))
            {
                CurrentValue = FinalValue;
                Progress = 0d;
                SetDisplayedState(CurrentValue, Progress);
                SetState(CountdownState.Completed);
                Invalidate();
                OnCountdownCompleted();
                return;
            }

            _timer!.Interval = StepInterval;
            _timer.Start();
            SetState(CountdownState.Running);
        }

        /// <summary>
        /// Pauses the countdown when it is running.
        /// </summary>
        public void Pause()
        {
            if (State != CountdownState.Running)
                return;

            _timer?.Stop();
            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(this);
            SetState(CountdownState.Paused);
            OnCountdownPaused();
        }

        /// <summary>
        /// Stops the countdown, cancels animations, and resets the value to <see cref="StartValue"/>.
        /// </summary>
        public void Stop()
        {
            _timer?.Stop();
            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(this);

            ResetToStartValue();
            SetState(CountdownState.Stopped);
            Invalidate();

            OnCountdownStopped();
        }

        #endregion

        #region Internal Helpers

        internal string GetFormattedDisplayText()
        {
            var valueToRender = Math.Round(_displayedValue, Math.Max(0, MaxDecimals), MidpointRounding.AwayFromZero);

            string text = TimeDisplayFormat switch
            {
                CountdownTimeDisplayFormat.MmSs => FormatAsMmSs(valueToRender),
                CountdownTimeDisplayFormat.HhMmSs => FormatAsHhMmSs(valueToRender),
                _ => FormatAsSeconds(valueToRender)
            };

            return string.Concat(text, ValueSuffix ?? string.Empty);
        }

        protected static float ToFloat(double value) => (float)value;

        #endregion

        #region Private Logic

        private static void OnVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is CountdownViewBase countdown)
            {
                countdown.Invalidate();
            }
        }

        private static void OnBehaviorPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is CountdownViewBase countdown)
            {
                countdown.HandleBehaviorPropertyChanged();
            }
        }

        private void HandleBehaviorPropertyChanged()
        {
            if (State == CountdownState.Running)
            {
                ValidateConfiguration();

                if (_timer is not null)
                {
                    _timer.Stop();
                    _timer.Interval = StepInterval;
                    _timer.Start();
                }

                Invalidate();
                return;
            }

            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(this);

            if (StartValue > 0)
            {
                ResetToStartValue();
            }

            Invalidate();
        }

        private void EnsureTimer()
        {
            if (_timer is not null)
                return;

            var dispatcher = Dispatcher ?? Application.Current?.Dispatcher;
            if (dispatcher is null)
                return;

            _timer = dispatcher.CreateTimer();
            _timer.Interval = StepInterval;
            _timer.IsRepeating = true;
            _timer.Tick += OnTimerTick;
        }

        private void HandleUnloaded(object? sender, EventArgs e)
        {
            CleanupLifecycleResources();
        }

        private void CleanupLifecycleResources()
        {
            StopAndDetachTimer();
            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(this);

            if (State is CountdownState.Running or CountdownState.Paused)
            {
                SetState(CountdownState.Stopped);

                if (StartValue > 0)
                {
                    ResetToStartValue();
                }

                Invalidate();
            }
        }

        private void StopAndDetachTimer()
        {
            if (_timer is null)
                return;

            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (State != CountdownState.Running)
                return;

            var previousValue = CurrentValue;
            var nextValue = CurrentValue - StepValue;

            if (nextValue < FinalValue)
            {
                nextValue = FinalValue;
            }

            var nextProgress = CalculateProgress(nextValue);

            CurrentValue = nextValue;
            Progress = nextProgress;

            UpdateDisplayedState(previousValue, nextValue, nextProgress);

            OnCountdownUpdated(previousValue, nextValue, nextProgress);

            if (AreClose(nextValue, FinalValue))
            {
                _timer?.Stop();
                SetState(CountdownState.Completed);
                OnCountdownCompleted();
            }
        }

        private void UpdateDisplayedState(double previousValue, double nextValue, double nextProgress)
        {
            var previousProgress = _displayedProgress;

            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(this);

            if (!Animate || AnimationDuration == 0)
            {
                SetDisplayedState(nextValue, nextProgress);
                Invalidate();
                return;
            }

            this.Animate(
                name: AnimationName,
                callback: progress =>
                {
                    _displayedValue = Lerp(previousValue, nextValue, progress);
                    _displayedProgress = Lerp(previousProgress, nextProgress, progress);
                    Invalidate();
                },
                start: 0d,
                end: 1d,
                rate: 16u,
                length: AnimationDuration,
                easing: Easing.Linear,
                finished: (_, _) =>
                {
                    SetDisplayedState(nextValue, nextProgress);
                    Invalidate();
                });
        }

        private void ResetToStartValue()
        {
            _isValueInitialized = true;
            CurrentValue = StartValue;
            Progress = CalculateProgress(StartValue);
            SetDisplayedState(CurrentValue, Progress);
        }

        private void SetDisplayedState(double value, double progress)
        {
            _displayedValue = value;
            _displayedProgress = Clamp01(progress);
        }

        private double CalculateProgress(double currentValue)
        {
            if (AreClose(StartValue, FinalValue))
                return 0d;

            var progress = (currentValue - FinalValue) / (StartValue - FinalValue);
            return Clamp01(progress);
        }

        private void ValidateConfiguration()
        {
            if (StartValue <= 0)
                throw new InvalidOperationException($"{nameof(StartValue)} must be greater than 0.");

            if (FinalValue < 0)
                throw new InvalidOperationException($"{nameof(FinalValue)} cannot be negative.");

            if (StartValue < FinalValue)
                throw new InvalidOperationException($"{nameof(StartValue)} must be greater than or equal to {nameof(FinalValue)}.");

            if (StepValue <= 0)
                throw new InvalidOperationException($"{nameof(StepValue)} must be greater than 0.");

            if (StepInterval <= TimeSpan.Zero)
                throw new InvalidOperationException($"{nameof(StepInterval)} must be greater than zero.");

            if (MaxDecimals < 0)
                throw new InvalidOperationException($"{nameof(MaxDecimals)} cannot be negative.");
        }

        private void SetState(CountdownState newState)
        {
            if (State == newState)
                return;

            State = newState;
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsCompleted));
        }

        private void OnCountdownStarted()
        {
            CountdownStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnCountdownPaused()
        {
            CountdownPaused?.Invoke(this, EventArgs.Empty);
        }

        private void OnCountdownStopped()
        {
            CountdownStopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnCountdownUpdated(double previousValue, double currentValue, double progress)
        {
            CountdownUpdated?.Invoke(this, new CountdownUpdatedEventArgs(previousValue, currentValue, progress));
        }

        private void OnCountdownCompleted()
        {
            CountdownCompleted?.Invoke(this, new CountdownCompletedEventArgs(FinalValue));
        }

        private string FormatAsSeconds(double value)
        {
            if (!string.IsNullOrWhiteSpace(TextFormat))
            {
                return string.Format(CultureInfo.CurrentCulture, TextFormat!, value);
            }

            var format = BuildMaxDecimalsFormat(MaxDecimals);
            return value.ToString(format, CultureInfo.CurrentCulture);
        }

        private static string FormatAsMmSs(double value)
        {
            var totalSeconds = Math.Max(0, (int)Math.Round(value, 0, MidpointRounding.AwayFromZero));
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

            return $"{minutes:00}:{seconds:00}";
        }

        private static string FormatAsHhMmSs(double value)
        {
            var totalSeconds = Math.Max(0, (int)Math.Round(value, 0, MidpointRounding.AwayFromZero));
            var time = TimeSpan.FromSeconds(totalSeconds);

            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        private static string BuildMaxDecimalsFormat(int maxDecimals)
        {
            if (maxDecimals <= 0)
                return "0";

            return "0." + new string('#', maxDecimals);
        }

        private static double Clamp01(double value) => Math.Max(0d, Math.Min(1d, value));

        private static bool AreClose(double left, double right) => Math.Abs(left - right) < Epsilon;

        private static double Lerp(double start, double end, double t) => start + ((end - start) * t);

        #endregion
    }
}
