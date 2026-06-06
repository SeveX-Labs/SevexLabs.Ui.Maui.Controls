namespace SevexLabs.Ui.Maui.Controls
{
    /// <summary>
    /// Pie-shaped countdown that inherits timer state and Start/Pause/Stop behavior from <see cref="CountdownViewBase"/>.
    /// </summary>
    public sealed class PieCountdown : CountdownViewBase
    {
        public static readonly BindableProperty CircleBackgroundColorProperty =
            BindableProperty.Create(
                nameof(CircleBackgroundColor),
                typeof(Color),
                typeof(PieCountdown),
                Colors.LightGray,
                propertyChanged: OnPieVisualPropertyChanged);

        public static readonly BindableProperty FillColorProperty =
            BindableProperty.Create(
                nameof(FillColor),
                typeof(Color),
                typeof(PieCountdown),
                Color.FromArgb("#5B9BD5"),
                propertyChanged: OnPieVisualPropertyChanged);

        public PieCountdown()
        {
            Drawable = new PieCountdownDrawable(this);
        }

        public Color CircleBackgroundColor
        {
            get => (Color)GetValue(CircleBackgroundColorProperty);
            set => SetValue(CircleBackgroundColorProperty, value);
        }

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        private static void OnPieVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is PieCountdown countdown)
            {
                countdown.Invalidate();
            }
        }
    }
}
