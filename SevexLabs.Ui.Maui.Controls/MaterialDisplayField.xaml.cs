using System.Windows.Input;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Read-only material-style display field with optional formatted content and error overlay.
/// </summary>
public partial class MaterialDisplayField : FastBorder
{
    private const string PlaceholderAnimName = "MDisp_Placeholder";

    private bool _loaded;
    private bool _lastHasValue;

    private enum ContentSource { None, Text, Formatted }
    private ContentSource _source = ContentSource.None;

    public MaterialDisplayField()
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
            typeof(MaterialDisplayField),
            new Thickness(0),
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialDisplayField)b;
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
            typeof(MaterialDisplayField),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).OnTextPropertyChanged((string?)n));

    /// <summary>
    /// Gets or sets the plain text displayed by the field.
    /// </summary>
    /// <remarks>
    /// <see cref="FormattedText"/> is the more specific visual source when it is present.
    /// </remarks>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty FormattedTextProperty =
        BindableProperty.Create(
            nameof(FormattedText),
            typeof(FormattedString),
            typeof(MaterialDisplayField),
            default(FormattedString),
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).OnFormattedTextPropertyChanged((FormattedString?)n));

    /// <summary>
    /// Gets or sets formatted text displayed by the field.
    /// </summary>
    /// <remarks>
    /// When present, this takes visual precedence over <see cref="Text"/>.
    /// </remarks>
    public FormattedString? FormattedText
    {
        get => (FormattedString?)GetValue(FormattedTextProperty);
        set => SetValue(FormattedTextProperty, value);
    }

    public static readonly BindableProperty PlaceholderUpYProperty =
        BindableProperty.Create(
            nameof(PlaceholderUpY),
            typeof(double),
            typeof(MaterialDisplayField),
            -10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialDisplayField)b;
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
            typeof(MaterialDisplayField),
            0.9,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialDisplayField)b;
                c.ApplyPlaceholderInstant();
            });

    public double PlaceholderScale
    {
        get => (double)GetValue(PlaceholderScaleProperty);
        set => SetValue(PlaceholderScaleProperty, value);
    }

    public static readonly BindableProperty ValueDownYProperty =
        BindableProperty.Create(
            nameof(ValueDownY),
            typeof(double),
            typeof(MaterialDisplayField),
            10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialDisplayField)b;
                c.ApplyPlaceholderInstant();
            });

    public double ValueDownY
    {
        get => (double)GetValue(ValueDownYProperty);
        set => SetValue(ValueDownYProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(MaterialDisplayField),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyPlaceholderText((string?)n));

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty UseFloatingPlaceholderProperty =
        BindableProperty.Create(
            nameof(UseFloatingPlaceholder),
            typeof(bool),
            typeof(MaterialDisplayField),
            true,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyPlaceholderMode());

    public bool UseFloatingPlaceholder
    {
        get => (bool)GetValue(UseFloatingPlaceholderProperty);
        set => SetValue(UseFloatingPlaceholderProperty, value);
    }

    public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(
            nameof(PlaceholderColor),
            typeof(Color),
            typeof(MaterialDisplayField),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyPlaceholderColor((Color)n));

    public Color PlaceholderColor
    {
        get => (Color)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    public static readonly BindableProperty HorizontalPlaceholderAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalPlaceholderAlignment),
            typeof(TextAlignment),
            typeof(MaterialDisplayField),
            TextAlignment.Start,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyPlaceholderAlignment());

    public TextAlignment HorizontalPlaceholderAlignment
    {
        get => (TextAlignment)GetValue(HorizontalPlaceholderAlignmentProperty);
        set => SetValue(HorizontalPlaceholderAlignmentProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialDisplayField),
            Colors.Black,
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyTextColor((Color)n));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialDisplayField),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyFontFamily((string?)n));

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly BindableProperty PlaceholderFontFamilyProperty =
        BindableProperty.Create(
            nameof(PlaceholderFontFamily),
            typeof(string),
            typeof(MaterialDisplayField),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyPlaceholderFontFamily((string?)n));

    public string? PlaceholderFontFamily
    {
        get => (string?)GetValue(PlaceholderFontFamilyProperty);
        set => SetValue(PlaceholderFontFamilyProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialDisplayField),
            18d,
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyFontSize((double)n));

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(MaterialDisplayField),
            TextAlignment.Start,
            propertyChanged: (b, _, n) => ((MaterialDisplayField)b).ApplyTextAlignment((TextAlignment)n));

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }
    
    public static readonly BindableProperty ErrorColorProperty =
        BindableProperty.Create(
            nameof(ErrorColor),
            typeof(Color),
            typeof(MaterialDisplayField),
            Colors.Red,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialDisplayField),
            2d,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialDisplayField),
            false,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyErrorBorderVisualState());

    /// <summary>
    /// Gets or sets whether the control shows its error border overlay.
    /// </summary>
    /// <remarks>
    /// The overlay does not overwrite the normal <see cref="FastBorder.BorderColor"/> or
    /// <see cref="FastBorder.BorderThickness="/>.
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
            typeof(MaterialDisplayField),
            0d,
            propertyChanged: (b, _, __) => ((MaterialDisplayField)b).ApplyTextIndent());

    public double TextIndent
    {
        get => (double)GetValue(TextIndentProperty);
        set => SetValue(TextIndentProperty, value);
    }

    // Tap command/event
    public static readonly BindableProperty FieldClickedCommandProperty =
        BindableProperty.Create(nameof(FieldClickedCommand), typeof(ICommand), typeof(MaterialDisplayField));

    public ICommand? FieldClickedCommand
    {
        get => (ICommand?)GetValue(FieldClickedCommandProperty);
        set => SetValue(FieldClickedCommandProperty, value);
    }

    public static readonly BindableProperty FieldClickedCommandParameterProperty =
        BindableProperty.Create(nameof(FieldClickedCommandParameter), typeof(object), typeof(MaterialDisplayField));

    public object? FieldClickedCommandParameter
    {
        get => GetValue(FieldClickedCommandParameterProperty);
        set => SetValue(FieldClickedCommandParameterProperty, value);
    }

    public event EventHandler<TappedEventArgs>? FieldTapped;

    // -----------------------
    // Apply methods
    // -----------------------

    private void ApplyAllVisualState()
    {
        // Value label
        ApplyTextColor(TextColor);
        ApplyFontFamily(FontFamily);
        ApplyFontSize(FontSize);
        ApplyTextAlignment(HorizontalTextAlignment);

        // Placeholder
        ApplyPlaceholderText(Placeholder);
        ApplyPlaceholderColor(PlaceholderColor);
        ApplyPlaceholderAlignment();
        ApplyPlaceholderMode();

        // HasError
        ApplyErrorBorderVisualState();

        // Indent
        ApplyTextIndent();

        // Initial content sync (last-wins visivo: Formatted se presente, altrimenti Text)
        ApplyContentFromProperties(initial: true);
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
        if (ValueLabel is null) return;
        ValueLabel.TextColor = c;
    }

    private void ApplyFontFamily(string? ff)
    {
        if (ValueLabel is null) return;

        ValueLabel.FontFamily = ff;

        // placeholder: se PlaceholderFontFamily non � settato, eredita
        if (string.IsNullOrEmpty(PlaceholderFontFamily))
            FloatingPlaceholderLabel.FontFamily = ff;
    }

    private void ApplyPlaceholderFontFamily(string? ff)
    {
        FloatingPlaceholderLabel.FontFamily = ff;
    }

    private void ApplyFontSize(double s)
    {
        if (ValueLabel is null) return;

        ValueLabel.FontSize = s;
        FloatingPlaceholderLabel.FontSize = s;

        if (!_loaded) return;
        ApplyPlaceholderInstant();
    }

    private void ApplyTextAlignment(TextAlignment a)
    {
        if (ValueLabel is null) return;
        ValueLabel.HorizontalTextAlignment = a;
        ApplyTextIndent();
    }

    private void ApplyPlaceholderAlignment()
    {
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

            default:
                FloatingPlaceholderLabel.HorizontalOptions = LayoutOptions.Start;
                FloatingPlaceholderLabel.AnchorX = 0;
                break;
        }

        ApplyTextIndent();
    }

    private void ApplyPlaceholderText(string? t)
    {
        FloatingPlaceholderLabel.Text = t ?? string.Empty;
    }

    private void ApplyPlaceholderColor(Color c)
    {
        FloatingPlaceholderLabel.TextColor = c;
    }

    private void ApplyPlaceholderMode()
    {
        // In questo controllo non esiste placeholder nativo: se disabiliti il floating
        // semplicemente nascondiamo la label placeholder.
        FloatingPlaceholderLabel.IsVisible = UseFloatingPlaceholder;

        if (_loaded)
            ApplyPlaceholderInstant();
    }

    private void ApplyTextIndent()
    {
        if (ValueLabel is null) return;

        var indent = Math.Max(0, TextIndent);

        // ValueLabel segue HorizontalTextAlignment
        double leftVal = 0;
        double rightVal = 0;

        switch (HorizontalTextAlignment)
        {
            case TextAlignment.End:
                rightVal += indent;
                break;
            case TextAlignment.Center:
                leftVal += indent;
                rightVal += indent;
                break;
            default:
                leftVal += indent;
                break;
        }

        ValueLabel.Margin = new Thickness(leftVal, 0, rightVal, 0);

        // Placeholder segue HorizontalPlaceholderAlignment
        ApplyFloatingPlaceholderIndent(indent);
    }

    private void ApplyFloatingPlaceholderIndent(double indent)
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
            default:
                left += indent;
                break;
        }

        FloatingPlaceholderLabel.Margin = new Thickness(left, 0, right, 0);
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
    // Content + placeholder state
    // -----------------------

    private void OnTextPropertyChanged(string? newText)
    {
        // Sorgente = Text (last-wins visivo)
        _source = ContentSource.Text;
        ApplyTextContent(newText);

        OnValuePresenceMayHaveChanged();
    }

    private void OnFormattedTextPropertyChanged(FormattedString? fs)
    {
        // normalizza "vuoto"
        if (!HasFormattedValue(fs))
            fs = null;

        // Sorgente = Formatted (last-wins visivo)
        _source = ContentSource.Formatted;
        ApplyFormattedContent(fs);

        OnValuePresenceMayHaveChanged();
    }

    private void ApplyContentFromProperties(bool initial)
    {
        // last-wins: se FormattedText � valorizzato, mostriamo quello,
        // altrimenti Text.
        if (HasFormattedValue(FormattedText))
        {
            _source = ContentSource.Formatted;
            ApplyFormattedContent(FormattedText);
        }
        else
        {
            _source = ContentSource.Text;
            ApplyTextContent(Text);
        }

        if (initial)
        {
            _lastHasValue = HasAnyValue(Text);
            ApplyPlaceholderInstant();
        }
        else
        {
            OnValuePresenceMayHaveChanged();
        }
    }

    private void ApplyTextContent(string? text)
    {
        if (ValueLabel is null) return;

        ValueLabel.FormattedText = null;
        ValueLabel.Text = text ?? string.Empty;

        ValueLabel.FontFamily = FontFamily;
        ValueLabel.FontSize = FontSize;
        ValueLabel.TextColor = TextColor;
    }

    private void ApplyFormattedContent(FormattedString? fs)
    {
        if (ValueLabel is null) return;

        ValueLabel.Text = string.Empty;
        ValueLabel.FormattedText = fs;

        ValueLabel.FontSize = FontSize;
        ValueLabel.TextColor = TextColor;
    }

    private bool HasFormattedValue(FormattedString? fs)
    {
        if (fs is null) return false;
        if (fs.Spans is null || fs.Spans.Count == 0) return false;

        foreach (var span in fs.Spans)
            if (!string.IsNullOrWhiteSpace(span?.Text))
                return true;

        return false;
    }

    private bool HasAnyValue(string? textSnapshot)
    {
        return _source switch
        {
            ContentSource.Formatted => HasFormattedValue(ValueLabel.FormattedText),
            ContentSource.Text => !string.IsNullOrEmpty(textSnapshot),
            _ => false
        };
    }

    private void OnValuePresenceMayHaveChanged()
    {
        if (!_loaded)
            return;

        if (!UseFloatingPlaceholder)
            return;

        var hasValue = HasAnyValue(Text);

        if (hasValue == _lastHasValue)
        {
            // nessuna transizione: comunque riallineo (es. font size / indent)
            ApplyPlaceholderInstant();
            return;
        }

        _lastHasValue = hasValue;

        if (hasValue)
            _ = AnimatePlaceholderUp();
        else
            _ = AnimatePlaceholderDown();
    }

    // -----------------------
    // Placeholder move (identico stile NewMaterialDisplayField)
    // -----------------------

    private void ApplyPlaceholderInstant()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        if (!UseFloatingPlaceholder)
        {
            FloatingPlaceholderLabel.IsVisible = false;
            ValueLabel.TranslationY = 0;
            return;
        }

        FloatingPlaceholderLabel.IsVisible = true;

        bool hasValue = HasAnyValue(Text);

        if (!hasValue)
        {
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = 0;
            FloatingPlaceholderLabel.Scale = 1;

            ValueLabel.TranslationY = 0;
        }
        else
        {
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = PlaceholderUpY;
            FloatingPlaceholderLabel.Scale = PlaceholderScale;

            ValueLabel.TranslationY = ValueDownY;
        }
    }

    private Task AnimatePlaceholderUp()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        var tcs = new TaskCompletionSource();

        var startPH_Y = FloatingPlaceholderLabel.TranslationY;
        var startPH_S = FloatingPlaceholderLabel.Scale;
        var startValueY = ValueLabel.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (PlaceholderUpY - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (PlaceholderScale - startPH_S) * progress;

                ValueLabel.TranslationY =
                    startValueY + (ValueDownY - startValueY) * progress;
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
        var startValueY = ValueLabel.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (0 - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (1 - startPH_S) * progress;

                ValueLabel.TranslationY =
                    startValueY + (0 - startValueY) * progress;
            },
            length: 250,
            easing: Easing.CubicOut,
            finished: (v, canceled) => tcs.TrySetResult()
        );

        return tcs.Task;
    }

    private void StopPlaceholderAnimations()
    {
        this.AbortAnimation(PlaceholderAnimName);

        FloatingPlaceholderLabel.AbortAnimation("TranslateTo");
        FloatingPlaceholderLabel.AbortAnimation("ScaleTo");
        ValueLabel.AbortAnimation("TranslateTo");
    }

    // -----------------------
    // Tap
    // -----------------------

    private void OnTapCatcherTapped(object? sender, EventArgs e)
    {
        var param = FieldClickedCommandParameter;

        if (FieldClickedCommand?.CanExecute(param) == true)
            FieldClickedCommand.Execute(param);

        FieldTapped?.Invoke(this, new TappedEventArgs(param));
    }

    private void HandleLoaded(object? sender, EventArgs e)
    {
        ApplyAllVisualState();
        _loaded = true;
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

}
