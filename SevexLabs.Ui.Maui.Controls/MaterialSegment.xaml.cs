namespace SevexLabs.Ui.Maui.Controls;

public partial class MaterialSegment
{
    #region bindable properties

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(MaterialSegment),
            false,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentTextProperty =
        BindableProperty.Create(
            nameof(SegmentText),
            typeof(string),
            typeof(MaterialSegment),
            string.Empty,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentWidthProperty =
        BindableProperty.Create(
            nameof(SegmentWidth),
            typeof(double),
            typeof(MaterialSegment),
            -1d,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentHeightProperty =
        BindableProperty.Create(
            nameof(SegmentHeight),
            typeof(double),
            typeof(MaterialSegment),
            -1d,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentPaddingProperty =
        BindableProperty.Create(
            nameof(SegmentPadding),
            typeof(Thickness),
            typeof(MaterialSegment),
            Thickness.Zero,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentRadiusProperty =
        BindableProperty.Create(
            nameof(SegmentRadius),
            typeof(CornerRadius),
            typeof(MaterialSegment),
            new CornerRadius(0),
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialSegment),
            default(string),
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialSegment),
            13d,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialSegment),
            Color.FromArgb("#484451"),
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedTextColorProperty =
        BindableProperty.Create(
            nameof(SelectedTextColor),
            typeof(Color),
            typeof(MaterialSegment),
            default(Color),
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SegmentBackgroundColorProperty =
        BindableProperty.Create(
            nameof(SegmentBackgroundColor),
            typeof(Color),
            typeof(MaterialSegment),
            Colors.Transparent,
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedSegmentBackgroundColorProperty =
        BindableProperty.Create(
            nameof(SelectedSegmentBackgroundColor),
            typeof(Color),
            typeof(MaterialSegment),
            default(Color),
            propertyChanged: (bindable, _, _) => ((MaterialSegment)bindable).ApplyVisualState());

    #endregion

    #region properties

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public string SegmentText
    {
        get => (string)GetValue(SegmentTextProperty);
        set => SetValue(SegmentTextProperty, value);
    }

    public double SegmentWidth
    {
        get => (double)GetValue(SegmentWidthProperty);
        set => SetValue(SegmentWidthProperty, value);
    }

    public double SegmentHeight
    {
        get => (double)GetValue(SegmentHeightProperty);
        set => SetValue(SegmentHeightProperty, value);
    }

    public Thickness SegmentPadding
    {
        get => (Thickness)GetValue(SegmentPaddingProperty);
        set => SetValue(SegmentPaddingProperty, value);
    }

    public CornerRadius SegmentRadius
    {
        get => (CornerRadius)GetValue(SegmentRadiusProperty);
        set => SetValue(SegmentRadiusProperty, value);
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

    public Color? SelectedTextColor
    {
        get => (Color?)GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public Color SegmentBackgroundColor
    {
        get => (Color)GetValue(SegmentBackgroundColorProperty);
        set => SetValue(SegmentBackgroundColorProperty, value);
    }

    public Color? SelectedSegmentBackgroundColor
    {
        get => (Color?)GetValue(SelectedSegmentBackgroundColorProperty);
        set => SetValue(SelectedSegmentBackgroundColorProperty, value);
    }

    #endregion

    #region ctor

    public MaterialSegment()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    #endregion

    #region overrides

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            ApplyVisualState();
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        ApplyVisualState();
    }

    #endregion

    #region event handlers

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplyVisualState();

        Dispatcher.Dispatch(ApplyVisualState);
    }

    #endregion

    #region helper methods

    internal void ApplyVisualState()
    {
        if (RootBorder is null || TitleLabel is null)
        {
            return;
        }

        var backgroundColor = IsSelected
            ? SelectedSegmentBackgroundColor ?? SegmentBackgroundColor
            : SegmentBackgroundColor;

        var textColor = IsSelected
            ? SelectedTextColor ?? TextColor
            : TextColor;

        RootBorder.WidthRequest = SegmentWidth;
        RootBorder.HeightRequest = SegmentHeight;
        RootBorder.Padding = SegmentPadding;
        RootBorder.CornerRadius = SegmentRadius;
        RootBorder.BackgroundColor = backgroundColor;

        TitleLabel.Text = SegmentText ?? string.Empty;
        TitleLabel.TextColor = textColor;
        TitleLabel.FontFamily = FontFamily;
        TitleLabel.FontSize = FontSize;

        RootBorder.InvalidateMeasure();
        TitleLabel.InvalidateMeasure();
        InvalidateMeasure();
    }

    #endregion
}