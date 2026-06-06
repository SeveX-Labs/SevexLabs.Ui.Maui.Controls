using SevexLabs.Ui.Maui.Controls.Model.Event_Args;
using System.Globalization;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Numeric entry with decrement/increment buttons and optional manual text entry.
/// </summary>
public partial class MaterialNumericEntry : FastBorder
{
    private bool _didUpdateFromButtons;
    private bool _didUpdateFromValue;
    private bool _isTextEditing;
    private bool _suppressFieldTextReaction;
    private bool _eventsSubscribed;
    
    public MaterialNumericEntry()
    {
        InitializeComponent();

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    // -----------------------
    // Bindable properties
    // -----------------------

    public new static readonly BindableProperty PaddingProperty =
        BindableProperty.Create(
            nameof(Padding),
            typeof(Thickness),
            typeof(MaterialNumericEntry),
            new Thickness(0),
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.RootGrid.Padding = (Thickness)n;
            });

    public new Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            nameof(Value),
            typeof(double?),
            typeof(MaterialNumericEntry),
            default(double?),
            BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((MaterialNumericEntry)b).OnValuePropertyChanged((double?)o, (double?)n));

    /// <summary>
    /// Gets or sets the numeric value represented by the control.
    /// </summary>
    /// <remarks>
    /// Values are clamped to <see cref="Minimum"/> and <see cref="Maximum"/> when synchronized.
    /// </remarks>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty StepProperty =
        BindableProperty.Create(nameof(Step), typeof(double), typeof(MaterialNumericEntry), 1d);

    /// <summary>
    /// Gets or sets the amount added or removed by the increment and decrement buttons.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(MaterialNumericEntry), double.NegativeInfinity);

    /// <summary>
    /// Gets or sets the lowest value accepted by the control.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(MaterialNumericEntry), double.PositiveInfinity);

    /// <summary>
    /// Gets or sets the highest value accepted by the control.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly BindableProperty DecimalPlacesProperty =
        BindableProperty.Create(nameof(DecimalPlaces), typeof(int?), typeof(MaterialNumericEntry), null);

    /// <summary>
    /// Gets or sets the number of decimal places used when formatting the value; null keeps the default formatting.
    /// </summary>
    public int? DecimalPlaces
    {
        get => (int?)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    // ---- UI + style forwarding (campo centrale & border) ----

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(MaterialNumericEntry),
            default(string),
            BindingMode.TwoWay,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).OnTextPropertyChanged((string?)n));

    /// <summary>
    /// Gets or sets the text shown in the inner entry.
    /// </summary>
    /// <remarks>
    /// Text is parsed back into <see cref="Value"/> when manual input is accepted.
    /// </remarks>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(MaterialNumericEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.Placeholder = (string?)n);

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.Black,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.TextColor = (Color)n);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(
            nameof(PlaceholderColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.PlaceholderColor = (Color)n);

    public Color PlaceholderColor
    {
        get => (Color)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialNumericEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.FontFamily = (string?)n);

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly BindableProperty PlaceholderFontFamilyProperty =
        BindableProperty.Create(
            nameof(PlaceholderFontFamily),
            typeof(string),
            typeof(MaterialNumericEntry),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.PlaceholderFontFamily = (string?)n);

    public string? PlaceholderFontFamily
    {
        get => (string?)GetValue(PlaceholderFontFamilyProperty);
        set => SetValue(PlaceholderFontFamilyProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialNumericEntry),
            18d,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.FontSize = (double)n);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty TextIndentProperty =
        BindableProperty.Create(
            nameof(TextIndent),
            typeof(double),
            typeof(MaterialNumericEntry),
            0d,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.TextIndent = (double)n);

    public double TextIndent
    {
        get => (double)GetValue(TextIndentProperty);
        set => SetValue(TextIndentProperty, value);
    }

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(MaterialNumericEntry),
            TextAlignment.End,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.HorizontalTextAlignment = (TextAlignment)n);

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(MaterialNumericEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyInputState());

    /// <summary>
    /// Gets or sets whether the whole control is read-only.
    /// </summary>
    /// <remarks>
    /// When true, manual input and the increment/decrement buttons cannot change the value.
    /// </remarks>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly BindableProperty IsManualInputDisabledProperty =
        BindableProperty.Create(
            nameof(IsManualInputDisabled),
            typeof(bool),
            typeof(MaterialNumericEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyInputState());

    /// <summary>
    /// Gets or sets whether direct typing in the inner entry is disabled.
    /// </summary>
    /// <remarks>
    /// This does not make the whole control read-only; the increment/decrement buttons remain usable.
    /// </remarks>
    public bool IsManualInputDisabled
    {
        get => (bool)GetValue(IsManualInputDisabledProperty);
        set => SetValue(IsManualInputDisabledProperty, value);
    }

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(MaterialNumericEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyHasError());

    /// <summary>
    /// Gets or sets whether the control shows its error border overlay.
    /// </summary>
    /// <remarks>
    /// The overlay is drawn separately from the normal <see cref="FastBorder.BorderColor"/> and
    /// <see cref="FastBorder.BorderThickness"/>.
    /// </remarks>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public static readonly BindableProperty ErrorColorProperty =
        BindableProperty.Create(
            nameof(ErrorColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.Red,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialNumericEntry),
            2d,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyErrorBorderVisualState());

    /// <summary>
    /// Gets or sets the thickness used by the error border overlay.
    /// </summary>
    public double ErrorBorderThickness
    {
        get => (double)GetValue(ErrorBorderThicknessProperty);
        set => SetValue(ErrorBorderThicknessProperty, value);
    }

    // Buttons styling
    public static readonly BindableProperty DecreaseBackgroundColorProperty =
        BindableProperty.Create(
            nameof(DecreaseBackgroundColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).MinusHost.BackgroundColor = (Color)n);

    public Color DecreaseBackgroundColor
    {
        get => (Color)GetValue(DecreaseBackgroundColorProperty);
        set => SetValue(DecreaseBackgroundColorProperty, value);
    }

    public static readonly BindableProperty IncreaseBackgroundColorProperty =
        BindableProperty.Create(
            nameof(IncreaseBackgroundColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).PlusHost.BackgroundColor = (Color)n);

    public Color IncreaseBackgroundColor
    {
        get => (Color)GetValue(IncreaseBackgroundColorProperty);
        set => SetValue(IncreaseBackgroundColorProperty, value);
    }

    public static readonly BindableProperty ButtonsTextColorProperty =
        BindableProperty.Create(
            nameof(ButtonsTextColor),
            typeof(Color),
            typeof(MaterialNumericEntry),
            Colors.Black,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.MinusLabel.TextColor = (Color)n;
                c.PlusLabel.TextColor = (Color)n;
            });

    public Color ButtonsTextColor
    {
        get => (Color)GetValue(ButtonsTextColorProperty);
        set => SetValue(ButtonsTextColorProperty, value);
    }

    public static readonly BindableProperty ButtonsFontFamilyProperty =
        BindableProperty.Create(
            nameof(ButtonsFontFamily),
            typeof(string),
            typeof(MaterialNumericEntry),
            default(string),
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.MinusLabel.FontFamily = (string?)n;
                c.PlusLabel.FontFamily = (string?)n;
            });

    public string? ButtonsFontFamily
    {
        get => (string?)GetValue(ButtonsFontFamilyProperty);
        set => SetValue(ButtonsFontFamilyProperty, value);
    }

    public static readonly BindableProperty ButtonsFontSizeProperty =
        BindableProperty.Create(
            nameof(ButtonsFontSize),
            typeof(double),
            typeof(MaterialNumericEntry),
            12d,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.MinusLabel.FontSize = (double)n;
                c.PlusLabel.FontSize = (double)n;
            });

    public double ButtonsFontSize
    {
        get => (double)GetValue(ButtonsFontSizeProperty);
        set => SetValue(ButtonsFontSizeProperty, value);
    }

    public static readonly BindableProperty DecreaseWidthProperty =
        BindableProperty.Create(
            nameof(DecreaseWidth),
            typeof(double),
            typeof(MaterialNumericEntry),
            44d,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).MinusLabel.WidthRequest = (double)n);

    public double DecreaseWidth
    {
        get => (double)GetValue(DecreaseWidthProperty);
        set => SetValue(DecreaseWidthProperty, value);
    }

    public static readonly BindableProperty IncreaseWidthProperty =
        BindableProperty.Create(
            nameof(IncreaseWidth),
            typeof(double),
            typeof(MaterialNumericEntry),
            44d,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).PlusLabel.WidthRequest = (double)n);

    public double IncreaseWidth
    {
        get => (double)GetValue(IncreaseWidthProperty);
        set => SetValue(IncreaseWidthProperty, value);
    }

    public static readonly BindableProperty EntryWidthProperty =
        BindableProperty.Create(
            nameof(EntryWidth),
            typeof(double),
            typeof(MaterialNumericEntry),
            -1d,
            propertyChanged: (b, _, n) => ((MaterialNumericEntry)b).Field.WidthRequest = (double)n);

    public double EntryWidth
    {
        get => (double)GetValue(EntryWidthProperty);
        set => SetValue(EntryWidthProperty, value);
    }

    // -----------------------
    // Forwarded properties (NewMaterialEntry behavior)
    // -----------------------

    public static readonly BindableProperty PlaceholderUpYProperty =
        BindableProperty.Create(
            nameof(PlaceholderUpY),
            typeof(double),
            typeof(MaterialNumericEntry),
            -10.0,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.Field.PlaceholderUpY = (double)n;
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
            typeof(MaterialNumericEntry),
            0.9,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.Field.PlaceholderScale = (double)n;
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
            typeof(MaterialNumericEntry),
            10.0,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.Field.EntryDownY = (double)n;
            });

    public double EntryDownY
    {
        get => (double)GetValue(EntryDownYProperty);
        set => SetValue(EntryDownYProperty, value);
    }

    public static readonly BindableProperty UseFloatingPlaceholderProperty =
        BindableProperty.Create(
            nameof(UseFloatingPlaceholder),
            typeof(bool),
            typeof(MaterialNumericEntry),
            true, // nel tuo XAML la imposti a True, quindi default coerente
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.Field.UseFloatingPlaceholder = (bool)n;
            });

    public bool UseFloatingPlaceholder
    {
        get => (bool)GetValue(UseFloatingPlaceholderProperty);
        set => SetValue(UseFloatingPlaceholderProperty, value);
    }

    public static readonly BindableProperty HorizontalPlaceholderAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalPlaceholderAlignment),
            typeof(TextAlignment),
            typeof(MaterialNumericEntry),
            TextAlignment.Start,
            propertyChanged: (b, _, n) =>
            {
                var c = (MaterialNumericEntry)b;
                c.Field.HorizontalPlaceholderAlignment = (TextAlignment)n;
            });

    public TextAlignment HorizontalPlaceholderAlignment
    {
        get => (TextAlignment)GetValue(HorizontalPlaceholderAlignmentProperty);
        set => SetValue(HorizontalPlaceholderAlignmentProperty, value);
    }

    public static readonly BindableProperty RaiseTapWhenReadOnlyProperty =
        BindableProperty.Create(
            nameof(RaiseTapWhenReadOnly),
            typeof(bool),
            typeof(MaterialNumericEntry),
            false,
            propertyChanged: (b, _, __) => ((MaterialNumericEntry)b).ApplyInputState());

    /// <summary>
    /// Gets or sets whether taps are raised while <see cref="IsReadOnly"/> is true.
    /// </summary>
    /// <remarks>
    /// This setting is tied to the true read-only state and is not enabled by <see cref="IsManualInputDisabled"/>.
    /// </remarks>
    public bool RaiseTapWhenReadOnly
    {
        get => (bool)GetValue(RaiseTapWhenReadOnlyProperty);
        set => SetValue(RaiseTapWhenReadOnlyProperty, value);
    }

    // -----------------------
    // Events
    // -----------------------

    /// <summary>
    /// Raised when the control is tapped while true read-only tap forwarding is active.
    /// </summary>
    public event EventHandler<TappedEventArgs>? ReadOnlyTapped;
    public event EventHandler<NullableControlValueChangedEventArgs>? ValueChanged;

    // -----------------------
    // Init
    // -----------------------

    private void ApplyHasError()
    {
        if (Field is not null)
        {
            Field.HasError = HasError;
            Field.ErrorBorderThickness = 0;
        }

        ApplyErrorBorderVisualState();
    }

    private void ApplyErrorBorderVisualState()
    {
        var errorThickness = Math.Max(0, ErrorBorderThickness);

        OverlayBorderColor = ErrorColor;
        OverlayBorderThickness = errorThickness;
        IsOverlayBorderVisible = HasError && errorThickness > 0;
    }

    private void ApplyAllVisualState()
    {
        MinusHost.BackgroundColor = DecreaseBackgroundColor;
        PlusHost.BackgroundColor = IncreaseBackgroundColor;

        Field.IsPassword = false;
        Field.ShowEye = false;
        Field.ErrorBorderThickness = 0;

        ApplyInputState();

        Field.UseFloatingPlaceholder = UseFloatingPlaceholder;
        Field.HorizontalPlaceholderAlignment = HorizontalPlaceholderAlignment;

        Field.PlaceholderUpY = PlaceholderUpY;
        Field.PlaceholderScale = PlaceholderScale;
        Field.EntryDownY = EntryDownY;

        ApplyHasError();
    }

    private void ApplyInputState()
    {
        Field.IsReadOnly = IsReadOnly || IsManualInputDisabled;
        Field.RaiseTapWhenReadOnly = IsReadOnly && RaiseTapWhenReadOnly;

        // Se read-only, i bottoni devono comunque �tappare� (se vuoi) ma non cambiare valore.
        // Qui li lasciamo tappabili ma senza effetto (vedi handlers).
    }

    // -----------------------
    // Value <-> Text
    // -----------------------

    private void OnValuePropertyChanged(double? oldValue, double? newValue)
    {
        if (_isTextEditing) return;
        if (_didUpdateFromButtons) return;

        _didUpdateFromValue = true;
        try
        {
            var clamped = newValue.HasValue ? Clamp(newValue.Value) : (double?)null;

            // Se clamp cambia, riallineo Value stesso (senza loop)
            if (newValue.HasValue && !AreClose(newValue.Value, clamped!.Value))
                SetValue(ValueProperty, clamped);

            SyncFieldFromValue(clamped, animate: false);

            if (!SameNullable(oldValue, clamped))
                ValueChanged?.Invoke(this, new NullableControlValueChangedEventArgs(BindingContext, oldValue, clamped));
        }
        finally
        {
            _didUpdateFromValue = false;
        }
    }

    private void OnTextPropertyChanged(string? newText)
    {
        if (_suppressFieldTextReaction) return;

        var safe = newText ?? string.Empty;
        if (Field.Text != safe)
        {
            _suppressFieldTextReaction = true;
            try { Field.Text = safe; }
            finally { _suppressFieldTextReaction = false; }
        }
    }

    private void SyncFieldFromValue(double? value, bool animate)
    {
        var culture = CultureInfo.CurrentCulture;

        var text = value.HasValue
            ? FormatNumber(value.Value, culture)
            : string.Empty;

        _suppressFieldTextReaction = true;
        try
        {
            Field.Text = text;
            SetValue(TextProperty, text);
        }
        finally
        {
            _suppressFieldTextReaction = false;
        }
    }

    // -----------------------
    // Buttons
    // -----------------------

    private void OnMinusTapped(object? sender, EventArgs e)
    {
        if (RaiseTapWhenReadOnly)
            ReadOnlyTapped?.Invoke(this, new TappedEventArgs(null));

        if (IsReadOnly) return;

        TryCommitTextToValue();

        if (_didUpdateFromButtons) return;
        _didUpdateFromButtons = true;

        try
        {
            var start = BaseForButtons();
            var old = Value;

            var next = start - Step;
            var clamped = Clamp(next);

            Value = clamped;
            SyncFieldFromValue(Value, animate: true);

            if (!SameNullable(old, Value))
                ValueChanged?.Invoke(this, new NullableControlValueChangedEventArgs(BindingContext, old, Value));
        }
        finally
        {
            _didUpdateFromButtons = false;
        }
    }

    private void OnPlusTapped(object? sender, EventArgs e)
    {
        if (RaiseTapWhenReadOnly)
            ReadOnlyTapped?.Invoke(this, new TappedEventArgs(null));

        if (IsReadOnly) return;

        TryCommitTextToValue();

        if (_didUpdateFromButtons) return;
        _didUpdateFromButtons = true;

        try
        {
            var start = BaseForButtons();
            var old = Value;

            var next = start + Step;
            var clamped = Clamp(next);

            Value = clamped;
            SyncFieldFromValue(Value, animate: true);

            if (!SameNullable(old, Value))
                ValueChanged?.Invoke(this, new NullableControlValueChangedEventArgs(BindingContext, old, Value));
        }
        finally
        {
            _didUpdateFromButtons = false;
        }
    }

    private double BaseForButtons()
    {
        // Se l'utente sta editando, prova SEMPRE dal testo corrente
        if (_isTextEditing)
        {
            var text = Field.Text?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                var culture = CultureInfo.CurrentCulture;
                if (double.TryParse(text, NumberStyles.Float, culture, out var parsed) &&
                    !double.IsNaN(parsed) && !double.IsInfinity(parsed))
                {
                    return parsed;
                }
            }
        }

        // Altrimenti usa Value (se valido)
        if (Value.HasValue && !double.IsNaN(Value.Value) && !double.IsInfinity(Value.Value))
            return Value.Value;

        // Fallback
        return double.IsNegativeInfinity(Minimum) ? 0d : Minimum;
    }

    // -----------------------
    // Manual typing
    // -----------------------

    private void OnFieldTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressFieldTextReaction) return;
        if (_didUpdateFromButtons || _didUpdateFromValue) return;

        var culture = CultureInfo.CurrentCulture;
        var decSep = culture.NumberFormat.NumberDecimalSeparator;

        var text = e.NewTextValue ?? string.Empty;
        bool allowMinus = double.IsNegativeInfinity(Minimum) || Minimum < 0;

        // 1) filtra caratteri non validi + normalizza separatore
        var filtered = FilterToNumeric(text, decSep, allowMinus, DecimalPlaces);
        if (filtered != text)
        {
            SetFieldTextInternal(filtered);
            text = filtered;
        }

        // 2) stati intermedi consentiti
        if (string.IsNullOrEmpty(text) || text == "-" || text == decSep || text == $"-{decSep}")
        {
            if (_isTextEditing) Value = null;
            return;
        }

        // 3) parse
        var normalized = text.Replace(".", decSep);
        if (double.TryParse(normalized, NumberStyles.Float, culture, out var parsed))
        {
            // ? BLOCCO SOLO MAX: non permettere valori > Maximum
            if (parsed > Maximum)
            {
                SetFieldTextInternal(e.OldTextValue ?? string.Empty);
                return;
            }

            // Aggiorna Value solo se stiamo editando davvero
            if (_isTextEditing)
                Value = parsed;
        }
        else
        {
            if (_isTextEditing)
            {
                Value = null;
                SetFieldTextInternal(e.OldTextValue ?? string.Empty);
            }
        }
    }

    private void OnFieldFocused(object? sender, FocusEventArgs e)
    {
        _isTextEditing = true;
    }

    private void OnFieldUnfocused(object? sender, FocusEventArgs e)
    {
        _isTextEditing = false;

        var culture = CultureInfo.CurrentCulture;
        var decSep = culture.NumberFormat.NumberDecimalSeparator;

        var text = Field.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            SetFieldTextInternal(string.Empty);
            Value = null;
            return;
        }

        if (text == "-" || text == decSep || text == $"-{decSep}")
        {
            var dv = DefaultValue();
            var clamped = Clamp(dv);
            Value = clamped;
            SyncFieldFromValue(Value, animate: false);
            return;
        }

        // Normalizzazione finale: clamp + format
        var parsed = Value ?? DefaultValue();
        var final = Clamp(parsed);

        if (!AreClose(Value.GetValueOrDefault(), final))
            Value = final;

        SyncFieldFromValue(Value, animate: false);
    }

    private void OnFieldReadOnlyTapped(object? sender, TappedEventArgs args)
    {
        ReadOnlyTapped?.Invoke(this, args);
    }

    private void SetFieldTextInternal(string text)
    {
        _suppressFieldTextReaction = true;
        try
        {
            Field.Text = text;
            SetValue(TextProperty, text);
        }
        finally
        {
            _suppressFieldTextReaction = false;
        }
    }

    // -----------------------
    // Helpers
    // -----------------------

    private double DefaultValue()
        => double.IsNegativeInfinity(Minimum) ? 0d : Minimum;

    private double Clamp(double v)
        => Math.Max(Minimum, Math.Min(Maximum, v));

    private static bool AreClose(double a, double b) => Math.Abs(a - b) <= 1e-12;

    private static bool SameNullable(double? a, double? b)
    {
        if (!a.HasValue && !b.HasValue) return true;
        if (a.HasValue != b.HasValue) return false;
        return AreClose(a!.Value, b!.Value);
    }

    private string FormatNumber(double v, CultureInfo culture)
    {
        double rounded = DecimalPlaces is { } dp
            ? Math.Round(v, dp, MidpointRounding.AwayFromZero)
            : v;

        if (Math.Abs(rounded - Math.Truncate(rounded)) <= 1e-12)
            return rounded.ToString("N0", culture);

        if (DecimalPlaces is { } dp2)
            return rounded.ToString("N" + dp2, culture);

        return rounded.ToString(culture);
    }

    private static string FilterToNumeric(string input, string decSep, bool allowMinus, int? decimalPlaces)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var result = new List<char>(input.Length);
        bool hasDec = false;
        int decimalsCount = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsDigit(c))
            {
                if (hasDec && decimalPlaces is { } dp && dp >= 0)
                {
                    if (decimalsCount >= dp) continue;
                    decimalsCount++;
                }

                result.Add(c);
                continue;
            }

            if ((c == '.' || c == ',') && !hasDec)
            {
                result.Add(decSep[0]);
                hasDec = true;
                decimalsCount = 0;
                continue;
            }

            if (c == '-' && allowMinus && i == 0)
            {
                result.Add(c);
                continue;
            }
        }

        // "-," -> "-0,"
        if (result.Count >= 2 && result[0] == '-' && result[1].ToString() == decSep)
            return "-" + "0" + decSep + new string(result.Skip(2).ToArray());

        return new string(result.ToArray());
    }

    private void HandleLoaded(object? sender, EventArgs e)
    {
        // iniziale sync
        SyncFieldFromValue(Value, animate: false);

        SubscribeEvents();
        ApplyAllVisualState();
    }

    private void HandleUnloaded(object? sender, EventArgs e)
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (_eventsSubscribed)
        {
            return;
        }

        Field.TextChanged += OnFieldTextChanged;
        Field.Focused += OnFieldFocused;
        Field.Unfocused += OnFieldUnfocused;
        Field.ReadOnlyTapped += OnFieldReadOnlyTapped;

        _eventsSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_eventsSubscribed)
        {
            return;
        }

        Field.TextChanged -= OnFieldTextChanged;
        Field.Focused -= OnFieldFocused;
        Field.Unfocused -= OnFieldUnfocused;
        Field.ReadOnlyTapped -= OnFieldReadOnlyTapped;

        _eventsSubscribed = false;
    }

    private bool TryCommitTextToValue()
    {
        var culture = CultureInfo.CurrentCulture;
        var decSep = culture.NumberFormat.NumberDecimalSeparator;

        var text = (Field.Text ?? string.Empty).Trim();

        // Stati intermedi: non committare
        if (string.IsNullOrEmpty(text) || text == "-" || text == decSep || text == $"-{decSep}")
            return false;

        // Parse
        var normalized = text.Replace(".", decSep);

        if (!double.TryParse(normalized, NumberStyles.Float, culture, out var parsed) ||
            double.IsNaN(parsed) || double.IsInfinity(parsed))
            return false;

        // Non permettere > Maximum (la tua regola)
        if (parsed > Maximum)
        {
            // Ripristina Value attuale (se c'�) o svuota; qui scegliamo il comportamento pi� "safe":
            SyncFieldFromValue(Value, animate: false);
            return false;
        }

        // Commit: aggiorna Value in modo coerente
        if (!Value.HasValue || !AreClose(Value.Value, parsed))
        {
            _didUpdateFromValue = true; // evita side effects mentre allinei
            try
            {
                Value = parsed;
            }
            finally
            {
                _didUpdateFromValue = false;
            }
        }

        return true;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(CornerRadius) ||
            propertyName == BorderThicknessProperty.PropertyName)
        {
            ApplyErrorBorderVisualState();
        }
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.OldHandler is not null)
        {
            UnsubscribeEvents();
        }

        base.OnHandlerChanging(args);
    }
}
