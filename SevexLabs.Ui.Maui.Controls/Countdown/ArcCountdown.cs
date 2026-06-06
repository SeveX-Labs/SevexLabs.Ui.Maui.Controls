namespace SevexLabs.Ui.Maui.Controls
{
    /// <summary>
    /// Arc-shaped countdown that inherits timer state and Start/Pause/Stop behavior from <see cref="CountdownViewBase"/>.
    /// </summary>
    public sealed class ArcCountdown : CountdownViewBase
    {
        public static readonly BindableProperty TrackColorProperty =
            BindableProperty.Create(
                nameof(TrackColor),
                typeof(Color),
                typeof(ArcCountdown),
                Colors.LightGray,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty TrackThicknessProperty =
            BindableProperty.Create(
                nameof(TrackThickness),
                typeof(double),
                typeof(ArcCountdown),
                18d,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty ProgressColorProperty =
            BindableProperty.Create(
                nameof(ProgressColor),
                typeof(Color),
                typeof(ArcCountdown),
                Color.FromArgb("#5B9BD5"),
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty StrokeCapProperty =
            BindableProperty.Create(
                nameof(StrokeCap),
                typeof(ArcStrokeCap),
                typeof(ArcCountdown),
                ArcStrokeCap.Round,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty CenterBackgroundColorProperty =
            BindableProperty.Create(
                nameof(CenterBackgroundColor),
                typeof(Color),
                typeof(ArcCountdown),
                Colors.Transparent,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty InnerArcColorProperty =
            BindableProperty.Create(
                nameof(InnerArcColor),
                typeof(Color),
                typeof(ArcCountdown),
                Colors.Transparent,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty InnerArcThicknessProperty =
            BindableProperty.Create(
                nameof(InnerArcThickness),
                typeof(double),
                typeof(ArcCountdown),
                0d,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty InnerArcOuterMarginProperty =
            BindableProperty.Create(
                nameof(InnerArcOuterMargin),
                typeof(double),
                typeof(ArcCountdown),
                0d,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty InnerArcInnerMarginProperty =
            BindableProperty.Create(
                nameof(InnerArcInnerMargin),
                typeof(double),
                typeof(ArcCountdown),
                0d,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty TrackShadowProperty =
            BindableProperty.Create(
                nameof(TrackShadow),
                typeof(Shadow),
                typeof(ArcCountdown),
                default(Shadow),
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty TrackUseShadowProperty =
            BindableProperty.Create(
                nameof(TrackUseShadow),
                typeof(bool),
                typeof(ArcCountdown),
                false,
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty TextShadowProperty =
            BindableProperty.Create(
                nameof(TextShadow),
                typeof(Shadow),
                typeof(ArcCountdown),
                default(Shadow),
                propertyChanged: OnArcVisualPropertyChanged);

        public static readonly BindableProperty TextUseShadowProperty =
            BindableProperty.Create(
                nameof(TextUseShadow),
                typeof(bool),
                typeof(ArcCountdown),
                false,
                propertyChanged: OnArcVisualPropertyChanged);

        public ArcCountdown()
        {
            Drawable = new ArcCountdownDrawable(this);
        }

        public Color TrackColor
        {
            get => (Color)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        public double TrackThickness
        {
            get => (double)GetValue(TrackThicknessProperty);
            set => SetValue(TrackThicknessProperty, value);
        }

        public Color ProgressColor
        {
            get => (Color)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        public ArcStrokeCap StrokeCap
        {
            get => (ArcStrokeCap)GetValue(StrokeCapProperty);
            set => SetValue(StrokeCapProperty, value);
        }

        public Color CenterBackgroundColor
        {
            get => (Color)GetValue(CenterBackgroundColorProperty);
            set => SetValue(CenterBackgroundColorProperty, value);
        }

        /// <summary>
        /// Color of the optional inner 360° arc drawn between the animated countdown ring and the center area.
        /// </summary>
        public Color InnerArcColor
        {
            get => (Color)GetValue(InnerArcColorProperty);
            set => SetValue(InnerArcColorProperty, value);
        }

        /// <summary>
        /// Thickness of the optional inner 360° arc.
        /// Set to 0 to hide the inner arc.
        /// </summary>
        public double InnerArcThickness
        {
            get => (double)GetValue(InnerArcThicknessProperty);
            set => SetValue(InnerArcThicknessProperty, value);
        }

        /// <summary>
        /// Transparent gap between the outer countdown ring and the optional inner arc.
        /// </summary>
        public double InnerArcOuterMargin
        {
            get => (double)GetValue(InnerArcOuterMarginProperty);
            set => SetValue(InnerArcOuterMarginProperty, value);
        }

        /// <summary>
        /// Transparent gap between the optional inner arc and the center background area.
        /// </summary>
        public double InnerArcInnerMargin
        {
            get => (double)GetValue(InnerArcInnerMarginProperty);
            set => SetValue(InnerArcInnerMarginProperty, value);
        }

        /// <summary>
        /// Shadow applied only to the outer track/progress ring.
        /// </summary>
        public Shadow? TrackShadow
        {
            get => (Shadow?)GetValue(TrackShadowProperty);
            set => SetValue(TrackShadowProperty, value);
        }

        public bool TrackUseShadow
        {
            get => (bool)GetValue(TrackUseShadowProperty);
            set => SetValue(TrackUseShadowProperty, value);
        }

        /// <summary>
        /// Shadow applied only to the countdown text.
        /// </summary>
        public Shadow? TextShadow
        {
            get => (Shadow?)GetValue(TextShadowProperty);
            set => SetValue(TextShadowProperty, value);
        }

        public bool TextUseShadow
        {
            get => (bool)GetValue(TextUseShadowProperty);
            set => SetValue(TextUseShadowProperty, value);
        }

        private static void OnArcVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is ArcCountdown countdown)
            {
                countdown.Invalidate();
            }
        }
    }
}
