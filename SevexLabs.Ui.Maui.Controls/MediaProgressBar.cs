using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using ViewExtensions = Microsoft.Maui.Controls.ViewExtensions;

namespace SevexLabs.Ui.Maui.Controls;

public enum MediaTimeDisplayFormat
{
    MmSs = 0,
    HhMmSs = 1
}

public enum MediaProgressBarTextHorizontalAlignment
{
    Start = 0,
    Center = 1,
    End = 2
}

public enum MediaProgressBarTimeLabelPosition
{
    Top = 0,
    Left = 1
}

/// <summary>
/// Time-oriented progress bar with countdown-like Start/Pause/Stop behavior.
/// </summary>
/// <remarks>
/// The value advances according to start/final values, step size, and timer interval. It is not
/// intended as a passive arbitrary progress indicator.
/// </remarks>
public sealed class MediaProgressBar : ContentView
{
    private const double Epsilon = 0.0000001d;
    private const string AnimationName = "MediaProgressBarAnimation";

    private IDispatcherTimer? _timer;

    private double _currentValue;
    private double _progress;
    private double _displayedValue;
    private double _displayedProgress;
    private bool _isValueInitialized;

    private readonly FastBorder _container;
    private readonly Grid _contentGrid;
    private readonly Label _timeLabel;
    private readonly AbsoluteLayout _progressHost;
    private readonly BoxView _trackBar;
    private readonly BoxView _progressBar;

    public MediaProgressBar()
    {
        _timeLabel = new Label
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };

        _trackBar = new BoxView
        {
            InputTransparent = true
        };

        _progressBar = new BoxView
        {
            InputTransparent = true
        };

        _progressHost = new AbsoluteLayout
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };

        AbsoluteLayout.SetLayoutFlags(_trackBar, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(_trackBar, new Rect(0, 0, 0, 0));

        AbsoluteLayout.SetLayoutFlags(_progressBar, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(_progressBar, new Rect(0, 0, 0, 0));

        _progressHost.Children.Add(_trackBar);
        _progressHost.Children.Add(_progressBar);

        _contentGrid = new Grid();

        ConfigureLayout();

        _container = new FastBorder
        {
            Content = _contentGrid
        };

        Content = _container;

        _progressHost.SizeChanged += OnProgressHostSizeChanged;
        Unloaded += HandleUnloaded;

        UpdateContainerVisuals();
        UpdateTextVisuals();
        UpdateProgressVisuals();
        SetDisplayedState(StartValue, CalculateProgress(StartValue));
        RefreshText();
        RefreshProgressBar();
    }

    #region BindableProperties

    public static readonly BindableProperty StartValueProperty =
        BindableProperty.Create(
            nameof(StartValue),
            typeof(double),
            typeof(MediaProgressBar),
            0d,
            propertyChanged: OnBehaviorPropertyChanged);

    public static readonly BindableProperty FinalValueProperty =
        BindableProperty.Create(
            nameof(FinalValue),
            typeof(double),
            typeof(MediaProgressBar),
            0d,
            propertyChanged: OnBehaviorPropertyChanged);

    public static readonly BindableProperty StepValueProperty =
        BindableProperty.Create(
            nameof(StepValue),
            typeof(double),
            typeof(MediaProgressBar),
            1d,
            propertyChanged: OnBehaviorPropertyChanged);

    public static readonly BindableProperty StepIntervalProperty =
        BindableProperty.Create(
            nameof(StepInterval),
            typeof(TimeSpan),
            typeof(MediaProgressBar),
            TimeSpan.FromSeconds(1),
            propertyChanged: OnBehaviorPropertyChanged);

    public static readonly BindableProperty DirectionProperty =
        BindableProperty.Create(
            nameof(Direction),
            typeof(CountdownDirection),
            typeof(MediaProgressBar),
            CountdownDirection.Clockwise,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty AnimateProperty =
        BindableProperty.Create(
            nameof(Animate),
            typeof(bool),
            typeof(MediaProgressBar),
            false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(
            nameof(AnimationDuration),
            typeof(uint),
            typeof(MediaProgressBar),
            (uint)300,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MediaProgressBar),
            default(string),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MediaProgressBar),
            16d,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MediaProgressBar),
            Colors.Black,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TimeHorizontalAlignmentProperty =
        BindableProperty.Create(
            nameof(TimeHorizontalAlignment),
            typeof(MediaProgressBarTextHorizontalAlignment),
            typeof(MediaProgressBar),
            MediaProgressBarTextHorizontalAlignment.Start,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TimeMarginProperty =
        BindableProperty.Create(
            nameof(TimeMargin),
            typeof(Thickness),
            typeof(MediaProgressBar),
            new Thickness(0),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TimeLabelPositionProperty =
        BindableProperty.Create(
            nameof(TimeLabelPosition),
            typeof(MediaProgressBarTimeLabelPosition),
            typeof(MediaProgressBar),
            MediaProgressBarTimeLabelPosition.Top,
            propertyChanged: OnLayoutPropertyChanged);

    public static readonly BindableProperty ProgressColorProperty =
        BindableProperty.Create(
            nameof(ProgressColor),
            typeof(Color),
            typeof(MediaProgressBar),
            Color.FromArgb("#5B9BD5"),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(
            nameof(TrackColor),
            typeof(Color),
            typeof(MediaProgressBar),
            Colors.LightGray,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ProgressBarHeightProperty =
        BindableProperty.Create(
            nameof(ProgressBarHeight),
            typeof(double),
            typeof(MediaProgressBar),
            8d,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ProgressBarCornerRadiusProperty =
        BindableProperty.Create(
            nameof(ProgressBarCornerRadius),
            typeof(CornerRadius),
            typeof(MediaProgressBar),
            new CornerRadius(999),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ProgressShadowProperty =
        BindableProperty.Create(
            nameof(ProgressShadow),
            typeof(Shadow),
            typeof(MediaProgressBar),
            default(Shadow),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ProgressUseShadowProperty =
        BindableProperty.Create(
            nameof(ProgressUseShadow),
            typeof(bool),
            typeof(MediaProgressBar),
            false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(
            nameof(Spacing),
            typeof(double),
            typeof(MediaProgressBar),
            8d,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TimeDisplayFormatProperty =
        BindableProperty.Create(
            nameof(TimeDisplayFormat),
            typeof(MediaTimeDisplayFormat),
            typeof(MediaProgressBar),
            MediaTimeDisplayFormat.MmSs,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ContainerBackgroundColor),
            typeof(Color),
            typeof(MediaProgressBar),
            Colors.Transparent,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerCornerRadiusProperty =
        BindableProperty.Create(
            nameof(ContainerCornerRadius),
            typeof(CornerRadius),
            typeof(MediaProgressBar),
            new CornerRadius(0d),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerPaddingProperty =
        BindableProperty.Create(
            nameof(ContainerPadding),
            typeof(Thickness),
            typeof(MediaProgressBar),
            new Thickness(0d),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerShadowProperty =
        BindableProperty.Create(
            nameof(ContainerShadow),
            typeof(Shadow),
            typeof(MediaProgressBar),
            default(Shadow),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerUseShadowProperty =
        BindableProperty.Create(
            nameof(ContainerUseShadow),
            typeof(bool),
            typeof(MediaProgressBar),
            false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerBorderColorProperty =
        BindableProperty.Create(
            nameof(ContainerBorderColor),
            typeof(Color),
            typeof(MediaProgressBar),
            Colors.Transparent,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ContainerBorderThicknessProperty =
        BindableProperty.Create(
            nameof(ContainerBorderThickness),
            typeof(double),
            typeof(MediaProgressBar),
            0d,
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

    public MediaProgressBarTextHorizontalAlignment TimeHorizontalAlignment
    {
        get => (MediaProgressBarTextHorizontalAlignment)GetValue(TimeHorizontalAlignmentProperty);
        set => SetValue(TimeHorizontalAlignmentProperty, value);
    }

    public Thickness TimeMargin
    {
        get => (Thickness)GetValue(TimeMarginProperty);
        set => SetValue(TimeMarginProperty, value);
    }

    public MediaProgressBarTimeLabelPosition TimeLabelPosition
    {
        get => (MediaProgressBarTimeLabelPosition)GetValue(TimeLabelPositionProperty);
        set => SetValue(TimeLabelPositionProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public double ProgressBarHeight
    {
        get => (double)GetValue(ProgressBarHeightProperty);
        set => SetValue(ProgressBarHeightProperty, value);
    }

    public CornerRadius ProgressBarCornerRadius
    {
        get => (CornerRadius)GetValue(ProgressBarCornerRadiusProperty);
        set => SetValue(ProgressBarCornerRadiusProperty, value);
    }

    public Shadow? ProgressShadow
    {
        get => (Shadow?)GetValue(ProgressShadowProperty);
        set => SetValue(ProgressShadowProperty, value);
    }

    public bool ProgressUseShadow
    {
        get => (bool)GetValue(ProgressUseShadowProperty);
        set => SetValue(ProgressUseShadowProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public MediaTimeDisplayFormat TimeDisplayFormat
    {
        get => (MediaTimeDisplayFormat)GetValue(TimeDisplayFormatProperty);
        set => SetValue(TimeDisplayFormatProperty, value);
    }

    public Color ContainerBackgroundColor
    {
        get => (Color)GetValue(ContainerBackgroundColorProperty);
        set => SetValue(ContainerBackgroundColorProperty, value);
    }

    public CornerRadius ContainerCornerRadius
    {
        get => (CornerRadius)GetValue(ContainerCornerRadiusProperty);
        set => SetValue(ContainerCornerRadiusProperty, value);
    }

    public Thickness ContainerPadding
    {
        get => (Thickness)GetValue(ContainerPaddingProperty);
        set => SetValue(ContainerPaddingProperty, value);
    }

    public Shadow? ContainerShadow
    {
        get => (Shadow?)GetValue(ContainerShadowProperty);
        set => SetValue(ContainerShadowProperty, value);
    }

    public bool ContainerUseShadow
    {
        get => (bool)GetValue(ContainerUseShadowProperty);
        set => SetValue(ContainerUseShadowProperty, value);
    }

    public Color ContainerBorderColor
    {
        get => (Color)GetValue(ContainerBorderColorProperty);
        set => SetValue(ContainerBorderColorProperty, value);
    }

    public double ContainerBorderThickness
    {
        get => (double)GetValue(ContainerBorderThicknessProperty);
        set => SetValue(ContainerBorderThicknessProperty, value);
    }

    public double CurrentValue
    {
        get => _currentValue;
        private set
        {
            if (AreClose(_currentValue, value))
                return;

            _currentValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Progress normalized between 0 and 1.
    /// 1 means full, 0 means completed.
    /// </summary>
    public double Progress
    {
        get => _progress;
        private set
        {
            var normalized = Clamp01(value);

            if (AreClose(_progress, normalized))
                return;

            _progress = normalized;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the current countdown-like state.
    /// </summary>
    public CountdownState State { get; private set; } = CountdownState.Stopped;

    /// <summary>
    /// Gets whether the progress timer is currently running.
    /// </summary>
    public bool IsRunning => State == CountdownState.Running;

    /// <summary>
    /// Gets whether the progress timer is paused.
    /// </summary>
    public bool IsPaused => State == CountdownState.Paused;

    /// <summary>
    /// Gets whether the progress timer has reached its final value.
    /// </summary>
    public bool IsCompleted => State == CountdownState.Completed;

    #endregion

    #region Events

    /// <summary>
    /// Raised when the progress timer starts.
    /// </summary>
    public event EventHandler? CountdownStarted;

    /// <summary>
    /// Raised when the progress timer pauses.
    /// </summary>
    public event EventHandler? CountdownPaused;

    /// <summary>
    /// Raised when the progress timer is explicitly stopped and reset.
    /// </summary>
    public event EventHandler? CountdownStopped;

    /// <summary>
    /// Raised after each timer step updates the current value and normalized progress.
    /// </summary>
    public event EventHandler<CountdownUpdatedEventArgs>? CountdownUpdated;

    /// <summary>
    /// Raised when the progress timer reaches its final value.
    /// </summary>
    public event EventHandler<CountdownCompletedEventArgs>? CountdownCompleted;

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
            RefreshVisuals();
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
    /// Starts a new timed progress run or resumes from a paused state.
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
            RefreshVisuals();
        }

        OnCountdownStarted();

        if (AreClose(StartValue, FinalValue) || AreClose(CurrentValue, FinalValue))
        {
            CurrentValue = FinalValue;
            Progress = 0d;
            SetDisplayedState(CurrentValue, Progress);
            SetState(CountdownState.Completed);
            RefreshVisuals();
            OnCountdownCompleted();
            return;
        }

        _timer!.Interval = StepInterval;
        _timer.Start();
        SetState(CountdownState.Running);
    }

    /// <summary>
    /// Pauses the timed progress run when it is currently running.
    /// </summary>
    public void Pause()
    {
        if (State != CountdownState.Running)
            return;

        _timer?.Stop();
        ViewExtensions.CancelAnimations(this);
        SetState(CountdownState.Paused);
        OnCountdownPaused();
    }

    /// <summary>
    /// Stops the timed progress run, cancels animations, and resets to <see cref="StartValue"/>.
    /// </summary>
    public void Stop()
    {
        _timer?.Stop();
        ViewExtensions.CancelAnimations(this);

        ResetToStartValue();
        SetState(CountdownState.Stopped);
        RefreshVisuals();

        OnCountdownStopped();
    }

    #endregion

    #region Private Static Callbacks

    private static void OnVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is MediaProgressBar control)
        {
            control.RefreshVisuals();
        }
    }

    private static void OnBehaviorPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is MediaProgressBar control)
        {
            control.HandleBehaviorPropertyChanged();
        }
    }

    private static void OnLayoutPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is MediaProgressBar control)
        {
            control.ConfigureLayout();
            control.RefreshVisuals();
        }
    }

    #endregion

    #region Private Logic

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

            RefreshVisuals();
            return;
        }

        ViewExtensions.CancelAnimations(this);

        if (StartValue > 0)
        {
            ResetToStartValue();
        }
        else
        {
            _isValueInitialized = false;
            CurrentValue = 0d;
            Progress = 0d;
            SetDisplayedState(0d, 0d);
        }

        RefreshVisuals();
    }

    private void ConfigureLayout()
    {
        _contentGrid.Children.Clear();
        _contentGrid.RowDefinitions.Clear();
        _contentGrid.ColumnDefinitions.Clear();
        _contentGrid.RowSpacing = 0d;
        _contentGrid.ColumnSpacing = 0d;

        if (TimeLabelPosition == MediaProgressBarTimeLabelPosition.Left)
        {
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            _contentGrid.Add(_timeLabel);
            Grid.SetColumn(_timeLabel, 0);
            Grid.SetRow(_timeLabel, 0);

            _contentGrid.Add(_progressHost);
            Grid.SetColumn(_progressHost, 1);
            Grid.SetRow(_progressHost, 0);
        }
        else
        {
            _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            _contentGrid.Add(_timeLabel);
            Grid.SetRow(_timeLabel, 0);
            Grid.SetColumn(_timeLabel, 0);

            _contentGrid.Add(_progressHost);
            Grid.SetRow(_progressHost, 1);
            Grid.SetColumn(_progressHost, 0);
        }
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
        ViewExtensions.CancelAnimations(this);

        if (State is CountdownState.Running or CountdownState.Paused)
        {
            SetState(CountdownState.Stopped);

            if (StartValue > 0)
            {
                ResetToStartValue();
            }
            else
            {
                _isValueInitialized = false;
                CurrentValue = 0d;
                Progress = 0d;
                SetDisplayedState(0d, 0d);
            }

            RefreshVisuals();
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

        this.CancelAnimations();

        if (!Animate || AnimationDuration == 0)
        {
            SetDisplayedState(nextValue, nextProgress);
            RefreshVisuals();
            return;
        }

        this.Animate(
            name: AnimationName,
            callback: progress =>
            {
                _displayedValue = Lerp(previousValue, nextValue, progress);
                _displayedProgress = Lerp(previousProgress, nextProgress, progress);
                RefreshVisuals();
            },
            start: 0d,
            end: 1d,
            rate: 16u,
            length: AnimationDuration,
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                SetDisplayedState(nextValue, nextProgress);
                RefreshVisuals();
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

        if (ProgressBarHeight < 0)
            throw new InvalidOperationException($"{nameof(ProgressBarHeight)} cannot be negative.");

        if (Spacing < 0)
            throw new InvalidOperationException($"{nameof(Spacing)} cannot be negative.");

        if (ContainerBorderThickness < 0)
            throw new InvalidOperationException($"{nameof(ContainerBorderThickness)} cannot be negative.");
    }

    private void RefreshVisuals()
    {
        UpdateContainerVisuals();
        UpdateTextVisuals();
        UpdateProgressVisuals();
        RefreshText();
        RefreshProgressBar();
    }

    private void UpdateContainerVisuals()
    {
        _container.BackgroundColor = ContainerBackgroundColor;
        _container.CornerRadius = ContainerCornerRadius;
        _container.Padding = ContainerPadding;
        _container.BorderColor = ContainerBorderColor;
        _container.BorderThickness = ContainerBorderThickness;
        _container.Shadow = ContainerUseShadow ? ContainerShadow : null;
    }

    private void UpdateTextVisuals()
    {
        _timeLabel.FontSize = FontSize;
        _timeLabel.TextColor = TextColor;
        _timeLabel.HorizontalTextAlignment = MapTextAlignment(TimeHorizontalAlignment);
        _timeLabel.Margin = TimeMargin;

        if (TimeLabelPosition == MediaProgressBarTimeLabelPosition.Left)
        {
            _timeLabel.HorizontalOptions = LayoutOptions.Start;
            _timeLabel.VerticalOptions = LayoutOptions.Center;
            _contentGrid.RowSpacing = 0d;
            _contentGrid.ColumnSpacing = Spacing;
        }
        else
        {
            _timeLabel.HorizontalOptions = LayoutOptions.Fill;
            _contentGrid.RowSpacing = Spacing;
            _contentGrid.ColumnSpacing = 0d;
        }

        if (string.IsNullOrWhiteSpace(FontFamily))
        {
            _timeLabel.FontFamily = null;
        }
        else
        {
            _timeLabel.FontFamily = FontFamily;
        }
    }

    private void UpdateProgressVisuals()
    {
        var height = Math.Max(0d, ProgressBarHeight);

        _progressHost.HeightRequest = height;
        _progressHost.HorizontalOptions = LayoutOptions.Fill;
        _progressHost.VerticalOptions = LayoutOptions.Center;

        _trackBar.BackgroundColor = TrackColor;
        _progressBar.BackgroundColor = ProgressColor;

        _trackBar.HeightRequest = height;
        _progressBar.HeightRequest = height;

        if (ProgressUseShadow && ProgressShadow != null)
        {
            _progressBar.Shadow = ProgressShadow;
        }
        else
        {
            _progressBar.Shadow = null;
        }

        ApplyCornerRadius(_trackBar, ProgressBarCornerRadius);
        ApplyCornerRadius(_progressBar, ProgressBarCornerRadius);

        RefreshProgressBar();
    }

    private void RefreshText()
    {
        _timeLabel.Text = FormatDisplayedTime(_displayedValue);
    }

    private void RefreshProgressBar()
    {
        var hostWidth = _progressHost.Width;
        var hostHeight = Math.Max(0d, ProgressBarHeight);

        if (hostWidth <= 0d || hostHeight <= 0d)
        {
            _progressBar.IsVisible = false;

            AbsoluteLayout.SetLayoutBounds(_trackBar, new Rect(0, 0, 0, 0));
            AbsoluteLayout.SetLayoutBounds(_progressBar, new Rect(0, 0, 0, 0));
            return;
        }

        _progressBar.IsVisible = true;

        AbsoluteLayout.SetLayoutBounds(_trackBar, new Rect(0, 0, hostWidth, hostHeight));

        var fillWidth = Math.Max(0d, hostWidth * Clamp01(_displayedProgress));
        var fillX = Direction == CountdownDirection.Clockwise
            ? 0d
            : Math.Max(0d, hostWidth - fillWidth);

        _progressBar.IsVisible = fillWidth > 0d;
        AbsoluteLayout.SetLayoutBounds(_progressBar, new Rect(fillX, 0, fillWidth, hostHeight));
    }

    private string FormatDisplayedTime(double value)
    {
        var safeValue = Math.Max(0d, value);
        var wholeSeconds = (int)Math.Round(safeValue, 0, MidpointRounding.AwayFromZero);
        var time = TimeSpan.FromSeconds(wholeSeconds);

        return TimeDisplayFormat switch
        {
            MediaTimeDisplayFormat.HhMmSs => $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}",
            _ => $"{(int)time.TotalMinutes:00}:{time.Seconds:00}"
        };
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

    private void OnProgressHostSizeChanged(object? sender, EventArgs e)
    {
        RefreshProgressBar();
    }

    private static TextAlignment MapTextAlignment(MediaProgressBarTextHorizontalAlignment alignment)
    {
        return alignment switch
        {
            MediaProgressBarTextHorizontalAlignment.Center => TextAlignment.Center,
            MediaProgressBarTextHorizontalAlignment.End => TextAlignment.End,
            _ => TextAlignment.Start
        };
    }

    private static void ApplyCornerRadius(BoxView boxView, CornerRadius cornerRadius)
    {
        boxView.CornerRadius = (float)Math.Max(
            0d,
            new[]
            {
                cornerRadius.TopLeft,
                cornerRadius.TopRight,
                cornerRadius.BottomLeft,
                cornerRadius.BottomRight
            }.Max());
    }

    private static double Clamp01(double value) => Math.Max(0d, Math.Min(1d, value));

    private static bool AreClose(double left, double right) => Math.Abs(left - right) < Epsilon;

    private static double Lerp(double start, double end, double t) => start + ((end - start) * t);

    #endregion
}
