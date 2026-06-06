namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Material-style single-line text input with optional floating placeholder and error overlay.
/// </summary>
public partial class MaterialEntry : FastBorder
{
    private const string PlaceholderAnimName = "MEntry_Placeholder";

    private const double DefaultEyePadding = 15d;
    private static readonly Lazy<ImageSource> EyeOpen = new(() => ImageSource.FromFile("eye_no"));
    private static readonly Lazy<ImageSource> EyeClosed = new(() => ImageSource.FromFile("eye_yes"));

    private bool _eyeOpen;
    private bool _loaded;
    private bool _suppressTextPropertyReaction;

    public MaterialEntry()
    {
        InitializeComponent();

        Loaded += HandleLoaded;
    }

    // -----------------------
    // Bindable properties
    // -----------------------

    public new static readonly BindableProperty PaddingProperty =
        BindableProperty.Create(
            nameof(Padding),
            typeof(Thickness),
            typeof(MaterialEntry),
            new Thickness(0),
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEntry)b;
                c.RootGrid.Padding = (Thickness)n;
            });

    public new Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(MaterialEntry),
            default(string),
            BindingMode.TwoWay,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).OnTextPropertyChanged((string?)n));

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty PlaceholderUpYProperty =
        BindableProperty.Create(
            nameof(PlaceholderUpY),
            typeof(double),
            typeof(MaterialEntry),
            -10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEntry)b;
                c.ApplyPlaceholderInstant();
            });

    public double PlaceholderUpY
    {
        get => (double)GetValue(PlaceholderUpYProperty);
        set => SetValue(PlaceholderUpYProperty, value);
    }

    public static readonly BindableProperty PlaceholderScaleProperty =
        BindableProperty.Create(
            nameof(PlaceholderScale),
            typeof(double),
            typeof(MaterialEntry),
            0.9,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEntry)b;
                c.ApplyPlaceholderInstant();
            });

    public double PlaceholderScale
    {
        get => (double)GetValue(PlaceholderScaleProperty);
        set => SetValue(PlaceholderScaleProperty, value);
    }

    public static readonly BindableProperty EntryDownYProperty =
        BindableProperty.Create(
            nameof(EntryDownY),
            typeof(double),
            typeof(MaterialEntry),
            10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEntry)b;
                c.ApplyPlaceholderInstant();
            });

    public double EntryDownY
    {
        get => (double)GetValue(EntryDownYProperty);
        set => SetValue(EntryDownYProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(MaterialEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyPlaceholderText((string?)n));

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty UseFloatingPlaceholderProperty =
        BindableProperty.Create(
            nameof(UseFloatingPlaceholder),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyPlaceholderMode());

    public bool UseFloatingPlaceholder
    {
        get => (bool)GetValue(UseFloatingPlaceholderProperty);
        set => SetValue(UseFloatingPlaceholderProperty, value);
    }

    public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(
            nameof(PlaceholderColor),
            typeof(Color),
            typeof(MaterialEntry),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyPlaceholderColor((Color)n));

    public Color PlaceholderColor
    {
        get => (Color)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    public static readonly BindableProperty HorizontalPlaceholderAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalPlaceholderAlignment),
            typeof(TextAlignment),
            typeof(MaterialEntry),
            TextAlignment.Start,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyPlaceholderAlignment());

    public TextAlignment HorizontalPlaceholderAlignment
    {
        get => (TextAlignment)GetValue(HorizontalPlaceholderAlignmentProperty);
        set => SetValue(HorizontalPlaceholderAlignmentProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialEntry),
            Colors.Black,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyTextColor((Color)n));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyFontFamily((string?)n));

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly BindableProperty PlaceholderFontFamilyProperty =
        BindableProperty.Create(
            nameof(PlaceholderFontFamily),
            typeof(string),
            typeof(MaterialEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyPlaceholderFontFamily((string?)n));

    public string? PlaceholderFontFamily
    {
        get => (string?)GetValue(PlaceholderFontFamilyProperty);
        set => SetValue(PlaceholderFontFamilyProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialEntry),
            18d,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyFontSize((double)n));

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(
            nameof(Keyboard),
            typeof(Keyboard),
            typeof(MaterialEntry),
            Keyboard.Default,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyKeyboard((Keyboard)n));

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(
            nameof(MaxLength),
            typeof(int),
            typeof(MaterialEntry),
            int.MaxValue,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyMaxLength((int)n));

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(MaterialEntry),
            TextAlignment.Start,
            propertyChanged: (b, _, n) => ((MaterialEntry)b).ApplyTextAlignment((TextAlignment)n));

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyReadOnlyState());

    /// <summary>
    /// Gets or sets whether the inner entry is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly BindableProperty RaiseTapWhenReadOnlyProperty =
        BindableProperty.Create(
            nameof(RaiseTapWhenReadOnly),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyReadOnlyState());

    /// <summary>
    /// Gets or sets whether taps on the read-only inner entry are exposed through <see cref="ReadOnlyTapped"/>.
    /// </summary>
    public bool RaiseTapWhenReadOnly
    {
        get => (bool)GetValue(RaiseTapWhenReadOnlyProperty);
        set => SetValue(RaiseTapWhenReadOnlyProperty, value);
    }

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(
            nameof(IsPassword),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyPasswordState());

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public static readonly BindableProperty ShowEyeProperty =
        BindableProperty.Create(
            nameof(ShowEye),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyPasswordState());

    public bool ShowEye
    {
        get => (bool)GetValue(ShowEyeProperty);
        set => SetValue(ShowEyeProperty, value);
    }

    // --- Border layer (FastBorder) ---
    public static readonly BindableProperty ErrorColorProperty =
        BindableProperty.Create(
            nameof(ErrorColor),
            typeof(Color),
            typeof(MaterialEntry),
            Colors.Red,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyErrorBorderVisualState());

    /// <summary>
    /// Gets or sets the color used by the error border overlay.
    /// </summary>
    public Color ErrorColor
    {
        get => (Color)GetValue(ErrorColorProperty);
        set => SetValue(ErrorColorProperty, value);
    }

    public static readonly BindableProperty ErrorBorderThicknessProperty =
        BindableProperty.Create(
            nameof(ErrorBorderThickness),
            typeof(double),
            typeof(MaterialEntry),
            2d,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyErrorBorderVisualState());

    /// <summary>
    /// Gets or sets the thickness used by the error border overlay.
    /// </summary>
    public double ErrorBorderThickness
    {
        get => (double)GetValue(ErrorBorderThicknessProperty);
        set => SetValue(ErrorBorderThicknessProperty, value);
    }

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(MaterialEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyErrorBorderVisualState());

    /// <summary>
    /// Gets or sets whether the control shows its error border overlay.
    /// </summary>
    /// <remarks>
    /// The overlay does not overwrite the normal <see cref="FastBorder.BorderColor"/> or
    /// <see cref="FastBorder.BorderThickness"/>.
    /// </remarks>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public static readonly BindableProperty TextIndentProperty =
    BindableProperty.Create(
        nameof(TextIndent),
        typeof(double),
        typeof(MaterialEntry),
        0d,
        propertyChanged: (b, _, __) => ((MaterialEntry)b).ApplyTextIndent());

    public double TextIndent
    {
        get => (double)GetValue(TextIndentProperty);
        set => SetValue(TextIndentProperty, value);
    }


    // Shadow/UseShadow li puoi lasciare direttamente ereditati da FastBorder
    // e NON ridefinirli qui: così anche gli Style su TargetType=NewMaterialEntry
    // possono settare UseShadow/Shadow senza “new”.

    // -----------------------
    // Events
    // -----------------------
    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler<FocusEventArgs>? EntryFocused;
    /// <summary>
    /// Raised when the read-only entry is tapped and <see cref="RaiseTapWhenReadOnly"/> is enabled.
    /// </summary>
    public event EventHandler<TappedEventArgs>? ReadOnlyTapped;

    // -----------------------
    // Apply methods
    // -----------------------

    private void ApplyAllVisualState()
    {
        // Entry
        ApplyTextColor(TextColor);
        ApplyFontFamily(FontFamily);
        ApplyFontSize(FontSize);
        ApplyKeyboard(Keyboard);
        ApplyMaxLength(MaxLength);
        ApplyTextAlignment(HorizontalTextAlignment);

        // Placeholder
        ApplyPlaceholderText(Placeholder);
        ApplyPlaceholderColor(PlaceholderColor);
        ApplyPlaceholderAlignment();
        ApplyPlaceholderMode();

        // Password
        ApplyPasswordState();

        // HasError
        ApplyErrorBorderVisualState();

        // ReadOnly
        ApplyReadOnlyState();

        // TextIndent
        ApplyTextIndent();

        // Text initial sync
        if (InnerEntry.Text != Text)
            InnerEntry.Text = Text;
    }

    private void ApplyErrorBorderVisualState()
    {
        var errorThickness = Math.Max(0, ErrorBorderThickness);

        OverlayBorderColor = ErrorColor;
        OverlayBorderThickness = errorThickness;
        IsOverlayBorderVisible = HasError && errorThickness > 0;
    }

    private void ApplyTextColor(Color c)
    {
        if (InnerEntry is null) return;
        InnerEntry.TextColor = c;
    }

    private void ApplyFontFamily(string? ff)
    {
        if (InnerEntry is null) return;
        InnerEntry.FontFamily = ff;

        // placeholder: se PlaceholderFontFamily non è settato, eredita
        if (string.IsNullOrEmpty(PlaceholderFontFamily))
            FloatingPlaceholderLabel.FontFamily = ff;
    }

    private void ApplyPlaceholderFontFamily(string? ff)
    {
        FloatingPlaceholderLabel.FontFamily = ff;
        if (!UseFloatingPlaceholder && InnerEntry is not null)
            InnerEntry.PlaceholderFontFamily = string.IsNullOrWhiteSpace(ff) ? FontFamily : ff;
    }

    private void ApplyFontSize(double s)
    {
        if (InnerEntry is null) return;
        InnerEntry.FontSize = s;
        FloatingPlaceholderLabel.FontSize = s;

        if (!_loaded) return;
        ApplyPlaceholderInstant();
    }

    private void ApplyKeyboard(Keyboard kb)
    {
        if (InnerEntry is null) return;
        InnerEntry.Keyboard = kb;
    }

    private void ApplyMaxLength(int ml)
    {
        if (InnerEntry is null) return;
        InnerEntry.MaxLength = ml;
    }

    private void ApplyTextAlignment(TextAlignment a)
    {
        if (InnerEntry is null) return;
        InnerEntry.HorizontalTextAlignment = a;
        ApplyTextIndent();
    }

    private void ApplyPlaceholderAlignment()
    {
        // placeholder custom NON deve seguire HorizontalTextAlignment
        // ma seguire HorizontalPlaceholderAlignment
        FloatingPlaceholderLabel.HorizontalTextAlignment = HorizontalPlaceholderAlignment;
        FloatingPlaceholderLabel.HorizontalOptions = HorizontalPlaceholderAlignment switch
        {
            TextAlignment.Center => LayoutOptions.Center,
            TextAlignment.End => LayoutOptions.End,
            _ => LayoutOptions.Start
        };

        switch (HorizontalPlaceholderAlignment)
        {
            case TextAlignment.Center:
                FloatingPlaceholderLabel.HorizontalOptions = LayoutOptions.Center;
                FloatingPlaceholderLabel.AnchorX = 0.5;
                break;

            case TextAlignment.End:
                FloatingPlaceholderLabel.HorizontalOptions = LayoutOptions.End;
                FloatingPlaceholderLabel.AnchorX = 1;
                break;

            default: // Start
                FloatingPlaceholderLabel.HorizontalOptions = LayoutOptions.Start;
                FloatingPlaceholderLabel.AnchorX = 0;
                break;
        }

        ApplyTextIndent();
    }

    private void ApplyPlaceholderText(string? t)
    {
        FloatingPlaceholderLabel.Text = t ?? string.Empty;
        if (!UseFloatingPlaceholder && InnerEntry is not null)
            InnerEntry.Placeholder = t ?? string.Empty;
    }

    private void ApplyPlaceholderColor(Color c)
    {
        FloatingPlaceholderLabel.TextColor = c;
        if (!UseFloatingPlaceholder && InnerEntry is not null)
            InnerEntry.PlaceholderColor = c;
    }

    private void ApplyPlaceholderMode()
    {
        if (InnerEntry is null) return;

        if (!UseFloatingPlaceholder)
        {
            FloatingPlaceholderLabel.IsVisible = false;
            InnerEntry.Placeholder = Placeholder ?? string.Empty;
            InnerEntry.PlaceholderColor = PlaceholderColor;
        }
        else
        {
            FloatingPlaceholderLabel.IsVisible = true;
            InnerEntry.Placeholder = string.Empty;
            ApplyPlaceholderInstant();
        }
    }

    private void ApplyReadOnlyState()
    {
        if (InnerEntry is not null)
            InnerEntry.IsReadOnly = IsReadOnly;

        if (ReadOnlyTapArea is not null)
        {
            var active = IsReadOnly && RaiseTapWhenReadOnly;
            ReadOnlyTapArea.IsVisible = active;
            ReadOnlyTapArea.InputTransparent = !active;
        }
    }

    private void ApplyPasswordState()
    {
        if (InnerEntry is null) return;

        var show = IsPassword && ShowEye;

        EyeContainer.IsVisible = show;   // <-- non più EyeImage
        ApplyEyePadding();               // <-- padding sul container
        ApplyTextIndent();               // <-- riserva spazio a destra

        if (!IsPassword)
        {
            _eyeOpen = false;
            InnerEntry.IsPassword = false;
            EyeImage.Source = null;
            return;
        }

        InnerEntry.IsPassword = !_eyeOpen;
        EyeImage.Source = _eyeOpen ? EyeOpen.Value : EyeClosed.Value;
    }

    private void ApplyTextIndent()
    {
        if (InnerEntry is null) return;

        var indent = Math.Max(0, TextIndent);
        var eyeReserve = GetEyeReservedWidth();

        // Padding della "zona contenuto" (FastBorder.Padding è già gestito dal Border)
        // Qui aggiungiamo solo il padding interno per testo/placeholder.
        double left = 0;
        double right = 0;

        // Testo + placeholder nativo Entry seguono HorizontalTextAlignment
        switch (HorizontalTextAlignment)
        {
            case TextAlignment.End:
                right += indent;
                break;

            case TextAlignment.Center:
                left += indent;
                right += indent;
                break;

            default: // Start
                left += indent;
                break;
        }

        // Spazio per l'occhio SEMPRE a destra (così il testo non ci va sotto)
        right += eyeReserve;

        // Applico alla Entry
        InnerEntry.Margin = new Thickness(left, 0, right, 0);

        // Floating placeholder: segue HorizontalPlaceholderAlignment (NON HorizontalTextAlignment)
        ApplyFloatingPlaceholderIndent(indent, eyeReserve);
    }

    private void ApplyFloatingPlaceholderIndent(double indent, double eyeReserve)
    {
        if (FloatingPlaceholderLabel is null) return;

        double left = 0;
        double right = 0;

        switch (HorizontalPlaceholderAlignment)
        {
            case TextAlignment.End:
                right += indent;
                break;

            case TextAlignment.Center:
                left += indent;
                right += indent;
                break;

            default: // Start
                left += indent;
                break;
        }

        // anche il placeholder deve evitare l'area dell'occhio
        right += eyeReserve;

        FloatingPlaceholderLabel.Margin = new Thickness(left, 0, right, 0);
    }

    private void ApplyEyePadding()
    {
        if (EyeContainer is null) return;

        var pad = GetEyePadding();
        EyeContainer.Padding = new Thickness(pad, 0, pad, 0);
    }

    private double GetEyePadding()
    {
        // “margine orizzontale” occhio = CornerRadius (se valorizzato) altrimenti 15
        // CornerRadius è CornerRadius per-angolo: prendiamo un valore “rappresentativo”
        var cr = CornerRadius;
        var max = Math.Max(Math.Max(cr.TopLeft, cr.TopRight), Math.Max(cr.BottomLeft, cr.BottomRight));
        return max > 0 ? max : DefaultEyePadding;
    }

    private double GetEyeReservedWidth()
    {
        if (!(IsPassword && ShowEye)) return 0;

        var imgW = EyeImage?.WidthRequest > 0 ? EyeImage.WidthRequest : 24;
        var pad = GetEyePadding();
        return imgW + (pad * 2);
    }

    private void EnsurePlaceholderAnchor()
    {
        FloatingPlaceholderLabel.AnchorX = HorizontalPlaceholderAlignment switch
        {
            TextAlignment.Center => 0.5,
            TextAlignment.End => 1.0,
            _ => 0.0
        };
    }

    // -----------------------
    // Text sync
    // -----------------------

    private void OnTextPropertyChanged(string? newText)
    {
        if (!_loaded) return;
        if (_suppressTextPropertyReaction) return;

        // arrivato dal VM: aggiorno Entry senza animazione “utente”
        if (InnerEntry.Text != newText)
        {
            _suppressTextPropertyReaction = true;
            try { InnerEntry.Text = newText; }
            finally { _suppressTextPropertyReaction = false; }
        }

        if (UseFloatingPlaceholder)
            ApplyPlaceholderInstant();
    }

    private void OnInnerEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            if (_suppressTextPropertyReaction)
                return;

            // questo è “utente” (o comunque cambiamento Entry). Propago in Text.
            if (Text != e.NewTextValue)
            {
                _suppressTextPropertyReaction = true;
                try { Text = e.NewTextValue; }
                finally { _suppressTextPropertyReaction = false; }
            }

            if (!UseFloatingPlaceholder)
                return;

            if (!string.IsNullOrEmpty(e.NewTextValue) && string.IsNullOrEmpty(e.OldTextValue))
            {
                _ = AnimatePlaceholderUp();
            }
            else if (string.IsNullOrEmpty(e.NewTextValue))
            {
                _ = AnimatePlaceholderDown();
            }
        }
        finally
        {
            TextChanged?.Invoke(this, e);
        }
    }

    // -----------------------
    // Placeholder move
    // -----------------------

    private void ApplyPlaceholderInstant()
    {
        if (!UseFloatingPlaceholder) return;

        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        bool hasText = !string.IsNullOrEmpty(Text);

        if (!hasText)
        {
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = 0;
            FloatingPlaceholderLabel.Scale = 1;

            InnerEntry.TranslationY = 0;
        }
        else
        {
            // Se vuoi che lo scale non “sposti a destra”, devi fissare l’anchor
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = PlaceholderUpY;
            FloatingPlaceholderLabel.Scale = PlaceholderScale;

            InnerEntry.TranslationY = EntryDownY;
        }
    }

    private Task AnimatePlaceholderUp()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        var tcs = new TaskCompletionSource();

        var startPH_Y = FloatingPlaceholderLabel.TranslationY;
        var startPH_S = FloatingPlaceholderLabel.Scale;
        var startEntryY = InnerEntry.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (PlaceholderUpY - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (PlaceholderScale - startPH_S) * progress;

                InnerEntry.TranslationY =
                    startEntryY + (EntryDownY - startEntryY) * progress;
            },
            length: 250,
            easing: Easing.CubicOut,
            finished: (v, canceled) => tcs.TrySetResult()
        );

        return tcs.Task;
    }

    private Task AnimatePlaceholderDown()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        var tcs = new TaskCompletionSource();

        var startPH_Y = FloatingPlaceholderLabel.TranslationY;
        var startPH_S = FloatingPlaceholderLabel.Scale;
        var startEntryY = InnerEntry.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (0 - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (1 - startPH_S) * progress;

                InnerEntry.TranslationY =
                    startEntryY + (0 - startEntryY) * progress;
            },
            length: 250,
            easing: Easing.CubicOut,
            finished: (v, canceled) => tcs.TrySetResult()
        );

        return tcs.Task;
    }

    private void StopPlaceholderAnimations()
    {
        // Ferma TUTTE le animazioni attive sulla label (TranslateTo/ScaleTo incluse)
        /*
        FloatingPlaceholderLabel.AbortAnimation("TranslationX");
        FloatingPlaceholderLabel.AbortAnimation("TranslationY");
        FloatingPlaceholderLabel.AbortAnimation("Scale");
        FloatingPlaceholderLabel.AbortAnimation("FadeTo");
        FloatingPlaceholderLabel.AbortAnimation("Rotation");
        FloatingPlaceholderLabel.AbortAnimation("RotationX");
        FloatingPlaceholderLabel.AbortAnimation("RotationY");
        */

        // E in più: abort “globale” per sicurezza
        this.AbortAnimation(PlaceholderAnimName);

        FloatingPlaceholderLabel.AbortAnimation("TranslateTo");
        FloatingPlaceholderLabel.AbortAnimation("ScaleTo");
    }

    private void OnInnerEntryFocused(object? sender, FocusEventArgs e)
    {
        EntryFocused?.Invoke(this, e);
    }
    private void OnInnerEntryUnfocused(object? sender, FocusEventArgs e)
    {
        EntryFocused?.Invoke(this, e);
    }

    private void OnEyeTapped(object? sender, EventArgs e)
    {
        if (!IsPassword) return;

        _eyeOpen = !_eyeOpen;
        ApplyPasswordState();
        InnerEntry.Focus();
    }

    private void OnReadOnlyTapped(object? sender, TappedEventArgs e)
    {
        if (!IsReadOnly || !RaiseTapWhenReadOnly) return;
        ReadOnlyTapped?.Invoke(this, e);
    }

    private async void OnTapCatcherTapped(object? sender, TappedEventArgs e)
    {
        if (!InnerEntry.IsEnabled || !InnerEntry.IsVisible)
            return;

        if (IsReadOnly)
        {
            if (RaiseTapWhenReadOnly)
                ReadOnlyTapped?.Invoke(this, e);
            return;
        }

        // Evita richieste multiple
        if (InnerEntry.IsFocused)
            return;

        // Android a volte vuole che la UI “si assesti” prima del focus
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Importante: evita che qualche overlay resti “selected”
            TapCatcher.IsEnabled = false;
            TapCatcher.IsEnabled = true;

            InnerEntry.Focus();
            await Task.Delay(10);
            InnerEntry.Focus();
        });
    }

    private void HandleLoaded(object? sender, EventArgs e)
    {
        ApplyAllVisualState();
        _loaded = true;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(CornerRadius))
        {
            ApplyEyePadding();
            ApplyTextIndent();
        }

        if (propertyName == nameof(CornerRadius) ||
            propertyName == BorderThicknessProperty.PropertyName)
        {
            ApplyErrorBorderVisualState();
        }
    }
}
