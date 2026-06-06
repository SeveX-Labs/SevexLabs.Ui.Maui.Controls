namespace SevexLabs.Ui.Maui.Controls;

public partial class MaterialFlexChip
{
    #region bindable properties

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(MaterialFlexChip),
            false,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipTextProperty =
        BindableProperty.Create(
            nameof(ChipText),
            typeof(string),
            typeof(MaterialFlexChip),
            string.Empty,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipWidthProperty =
        BindableProperty.Create(
            nameof(ChipWidth),
            typeof(double),
            typeof(MaterialFlexChip),
            -1d,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipHeightProperty =
        BindableProperty.Create(
            nameof(ChipHeight),
            typeof(double),
            typeof(MaterialFlexChip),
            -1d,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipPaddingProperty =
        BindableProperty.Create(
            nameof(ChipPadding),
            typeof(Thickness),
            typeof(MaterialFlexChip),
            Thickness.Zero,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipRadiusProperty =
        BindableProperty.Create(
            nameof(ChipRadius),
            typeof(CornerRadius),
            typeof(MaterialFlexChip),
            new CornerRadius(0),
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialFlexChip),
            default(string),
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialFlexChip),
            13d,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            Color.FromArgb("#484451"),
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedTextColorProperty =
        BindableProperty.Create(
            nameof(SelectedTextColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            null,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty ChipBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ChipBackgroundColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            Colors.Transparent,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedChipBackgroundColorProperty =
        BindableProperty.Create(
            nameof(SelectedChipBackgroundColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            null,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty BorderThicknessProperty =
        BindableProperty.Create(
            nameof(BorderThickness),
            typeof(double),
            typeof(MaterialFlexChip),
            1d,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedBorderThicknessProperty =
        BindableProperty.Create(
            nameof(SelectedBorderThickness),
            typeof(double),
            typeof(MaterialFlexChip),
            -1d,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(
            nameof(BorderColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            Color.FromArgb("#484451"),
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    public static readonly BindableProperty SelectedBorderColorProperty =
        BindableProperty.Create(
            nameof(SelectedBorderColor),
            typeof(Color),
            typeof(MaterialFlexChip),
            null,
            propertyChanged: (bindable, _, _) => ((MaterialFlexChip)bindable).ApplyVisualState());

    #endregion

    #region properties

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public string ChipText
    {
        get => (string)GetValue(ChipTextProperty);
        set => SetValue(ChipTextProperty, value);
    }

    public double ChipWidth
    {
        get => (double)GetValue(ChipWidthProperty);
        set => SetValue(ChipWidthProperty, value);
    }

    public double ChipHeight
    {
        get => (double)GetValue(ChipHeightProperty);
        set => SetValue(ChipHeightProperty, value);
    }

    public Thickness ChipPadding
    {
        get => (Thickness)GetValue(ChipPaddingProperty);
        set => SetValue(ChipPaddingProperty, value);
    }

    public CornerRadius ChipRadius
    {
        get => (CornerRadius)GetValue(ChipRadiusProperty);
        set => SetValue(ChipRadiusProperty, value);
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

    public Color ChipBackgroundColor
    {
        get => (Color)GetValue(ChipBackgroundColorProperty);
        set => SetValue(ChipBackgroundColorProperty, value);
    }

    public Color? SelectedChipBackgroundColor
    {
        get => (Color?)GetValue(SelectedChipBackgroundColorProperty);
        set => SetValue(SelectedChipBackgroundColorProperty, value);
    }

    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public double SelectedBorderThickness
    {
        get => (double)GetValue(SelectedBorderThicknessProperty);
        set => SetValue(SelectedBorderThicknessProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public Color? SelectedBorderColor
    {
        get => (Color?)GetValue(SelectedBorderColorProperty);
        set => SetValue(SelectedBorderColorProperty, value);
    }

    #endregion

    #region ctor

    public MaterialFlexChip()
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

    private void ApplyVisualState()
    {
        if (RootBorder is null || TitleLabel is null)
        {
            return;
        }

        var backgroundColor = IsSelected
            ? SelectedChipBackgroundColor ?? ChipBackgroundColor
            : ChipBackgroundColor;

        var textColor = IsSelected
            ? SelectedTextColor ?? TextColor
            : TextColor;

        var borderColor = IsSelected
            ? SelectedBorderColor ?? BorderColor
            : BorderColor;

        var borderThickness = IsSelected && SelectedBorderThickness >= 0
            ? SelectedBorderThickness
            : BorderThickness;

        RootBorder.WidthRequest = ChipWidth;
        RootBorder.HeightRequest = ChipHeight;
        RootBorder.Padding = ChipPadding;
        RootBorder.CornerRadius = ChipRadius;
        RootBorder.BackgroundColor = backgroundColor;
        RootBorder.BorderColor = borderColor;
        RootBorder.BorderThickness = borderThickness;

        TitleLabel.Text = ChipText ?? string.Empty;
        TitleLabel.TextColor = textColor;
        TitleLabel.FontFamily = FontFamily;
        TitleLabel.FontSize = FontSize;

        RootBorder.IsVisible = true;
        RootBorder.Opacity = 1;

        TitleLabel.IsVisible = true;
        TitleLabel.Opacity = 1;

        RootBorder.InvalidateMeasure();
        TitleLabel.InvalidateMeasure();
        InvalidateMeasure();
    }

    #endregion
}