namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Material-style multi-line text input with optional floating placeholder and error overlay.
/// </summary>
public partial class MaterialEditor : FastBorder
{
    private const string PlaceholderAnimName = "MEditor_Placeholder";

    private bool _loaded;
    private bool _suppressTextPropertyReaction;

    public MaterialEditor()
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
            typeof(MaterialEditor),
            new Thickness(0),
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEditor)b;
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
            typeof(MaterialEditor),
            default(string),
            BindingMode.TwoWay,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).OnTextPropertyChanged((string?)n));

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty PlaceholderUpYProperty =
        BindableProperty.Create(
            nameof(PlaceholderUpY),
            typeof(double),
            typeof(MaterialEditor),
            -10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEditor)b;
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
            typeof(MaterialEditor),
            0.9,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEditor)b;
                c.ApplyPlaceholderInstant();
            });

    public double PlaceholderScale
    {
        get => (double)GetValue(PlaceholderScaleProperty);
        set => SetValue(PlaceholderScaleProperty, value);
    }

    public static readonly BindableProperty EditorDownYProperty =
        BindableProperty.Create(
            nameof(EditorDownY),
            typeof(double),
            typeof(MaterialEditor),
            10.0,
            propertyChanged: (b, o, n) =>
            {
                var c = (MaterialEditor)b;
                c.ApplyPlaceholderInstant();
            });

    public double EditorDownY
    {
        get => (double)GetValue(EditorDownYProperty);
        set => SetValue(EditorDownYProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(MaterialEditor),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyPlaceholderText((string?)n));

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty UseFloatingPlaceholderProperty =
        BindableProperty.Create(
            nameof(UseFloatingPlaceholder),
            typeof(bool),
            typeof(MaterialEditor),
            false,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyPlaceholderMode());

    public bool UseFloatingPlaceholder
    {
        get => (bool)GetValue(UseFloatingPlaceholderProperty);
        set => SetValue(UseFloatingPlaceholderProperty, value);
    }

    public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(
            nameof(PlaceholderColor),
            typeof(Color),
            typeof(MaterialEditor),
            Colors.LightGray,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyPlaceholderColor((Color)n));

    public Color PlaceholderColor
    {
        get => (Color)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    public static readonly BindableProperty HorizontalPlaceholderAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalPlaceholderAlignment),
            typeof(TextAlignment),
            typeof(MaterialEditor),
            TextAlignment.Start,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyPlaceholderAlignment());

    public TextAlignment HorizontalPlaceholderAlignment
    {
        get => (TextAlignment)GetValue(HorizontalPlaceholderAlignmentProperty);
        set => SetValue(HorizontalPlaceholderAlignmentProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialEditor),
            Colors.Black,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyTextColor((Color)n));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialEditor),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyFontFamily((string?)n));

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly BindableProperty PlaceholderFontFamilyProperty =
        BindableProperty.Create(
            nameof(PlaceholderFontFamily),
            typeof(string),
            typeof(MaterialEditor),
            default(string),
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyPlaceholderFontFamily((string?)n));

    public string? PlaceholderFontFamily
    {
        get => (string?)GetValue(PlaceholderFontFamilyProperty);
        set => SetValue(PlaceholderFontFamilyProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialEditor),
            18d,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyFontSize((double)n));

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(
            nameof(Keyboard),
            typeof(Keyboard),
            typeof(MaterialEditor),
            Keyboard.Default,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyKeyboard((Keyboard)n));

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(MaterialEditor),
            TextAlignment.Start,
            propertyChanged: (b, _, n) => ((MaterialEditor)b).ApplyTextAlignment((TextAlignment)n));

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(MaterialEditor),
            false,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyReadOnlyState());

    /// <summary>
    /// Gets or sets whether the inner editor is read-only.
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
            typeof(MaterialEditor),
            false,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyReadOnlyState());

    /// <summary>
    /// Gets or sets whether taps on the read-only inner editor are exposed through <see cref="ReadOnlyTapped"/>.
    /// </summary>
    public bool RaiseTapWhenReadOnly
    {
        get => (bool)GetValue(RaiseTapWhenReadOnlyProperty);
        set => SetValue(RaiseTapWhenReadOnlyProperty, value);
    }
    
    public static readonly BindableProperty ErrorColorProperty =
        BindableProperty.Create(
            nameof(ErrorColor),
            typeof(Color),
            typeof(MaterialEditor),
            Colors.Red,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialEditor),
            2d,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialEditor),
            false,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyErrorBorderVisualState());

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
            typeof(MaterialEditor),
            0d,
            propertyChanged: (b, _, __) => ((MaterialEditor)b).ApplyTextIndent());

    public double TextIndent
    {
        get => (double)GetValue(TextIndentProperty);
        set => SetValue(TextIndentProperty, value);
    }

    // -----------------------
    // Events
    // -----------------------
    /// <summary>
    /// Raised when the read-only editor is tapped and <see cref="RaiseTapWhenReadOnly"/> is enabled.
    /// </summary>
    public event EventHandler? ReadOnlyTapped;

    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler<FocusEventArgs>? EditorFocused;

    // -----------------------
    // Apply methods
    // -----------------------

    private void ApplyAllVisualState()
    {
        // Editor
        ApplyTextColor(TextColor);
        ApplyFontFamily(FontFamily);
        ApplyFontSize(FontSize);
        ApplyKeyboard(Keyboard);
        ApplyTextAlignment(HorizontalTextAlignment);

        // Placeholder
        ApplyPlaceholderText(Placeholder);
        ApplyPlaceholderColor(PlaceholderColor);
        ApplyPlaceholderAlignment();
        ApplyPlaceholderMode();

        // HasError
        ApplyErrorBorderVisualState();

        // ReadOnly
        ApplyReadOnlyState();

        // Indent
        ApplyTextIndent();

        // Text initial sync
        if (InnerEditor.Text != Text)
            InnerEditor.Text = Text;
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
        if (InnerEditor is null) return;
        InnerEditor.TextColor = c;
    }

    private void ApplyFontFamily(string? ff)
    {
        if (InnerEditor is null) return;

        InnerEditor.FontFamily = ff;

        if (string.IsNullOrEmpty(PlaceholderFontFamily))
            FloatingPlaceholderLabel.FontFamily = ff;
    }

    private void ApplyPlaceholderFontFamily(string? ff)
    {
        FloatingPlaceholderLabel.FontFamily = ff;
    }

    private void ApplyFontSize(double s)
    {
        if (InnerEditor is null) return;

        InnerEditor.FontSize = s;
        FloatingPlaceholderLabel.FontSize = s;

        if (!_loaded) return;
        ApplyPlaceholderInstant();
    }

    private void ApplyKeyboard(Keyboard kb)
    {
        if (InnerEditor is null) return;
        InnerEditor.Keyboard = kb;
    }

    private void ApplyTextAlignment(TextAlignment a)
    {
        if (InnerEditor is null) return;
        InnerEditor.HorizontalTextAlignment = a;
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
        var placeholder = t ?? string.Empty;

        FloatingPlaceholderLabel.Text = placeholder;

        if (InnerEditor is null)
            return;

        if (UseFloatingPlaceholder)
            InnerEditor.Placeholder = string.Empty;
        else
            InnerEditor.Placeholder = placeholder;
    }

    private void ApplyPlaceholderColor(Color c)
    {
        FloatingPlaceholderLabel.TextColor = c;

        if (InnerEditor is null)
            return;

        InnerEditor.PlaceholderColor = c;
    }

    private void ApplyPlaceholderMode()
    {
        if (InnerEditor is null)
            return;

        FloatingPlaceholderLabel.IsVisible = UseFloatingPlaceholder;

        if (UseFloatingPlaceholder)
        {
            InnerEditor.Placeholder = string.Empty;
        }
        else
        {
            InnerEditor.Placeholder = Placeholder ?? string.Empty;
            InnerEditor.TranslationY = 0;
        }

        if (_loaded)
            ApplyPlaceholderInstant();
    }

    private void ApplyReadOnlyState()
    {
        if (InnerEditor is not null)
            InnerEditor.IsReadOnly = IsReadOnly;

        if (ReadOnlyTapArea is not null)
        {
            var active = IsReadOnly && RaiseTapWhenReadOnly;
            ReadOnlyTapArea.IsVisible = active;
            ReadOnlyTapArea.InputTransparent = !active;
        }
    }

    private void ApplyTextIndent()
    {
        if (InnerEditor is null) return;

        var indent = Math.Max(0, TextIndent);

        double left = 0;
        double right = 0;

        switch (HorizontalTextAlignment)
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

        InnerEditor.Margin = new Thickness(left, 0, right, 0);

        ApplyFloatingPlaceholderIndent(indent);
    }

    private void ApplyFloatingPlaceholderIndent(double indent)
    {
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
    // Text sync
    // -----------------------

    private void OnTextPropertyChanged(string? newText)
    {
        if (!_loaded) return;
        if (_suppressTextPropertyReaction) return;

        if (InnerEditor.Text != newText)
        {
            _suppressTextPropertyReaction = true;
            try { InnerEditor.Text = newText; }
            finally { _suppressTextPropertyReaction = false; }
        }

        if (UseFloatingPlaceholder)
            ApplyPlaceholderInstant();
    }

    private void OnInnerEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressTextPropertyReaction)
            return;

        if (Text != e.NewTextValue)
        {
            _suppressTextPropertyReaction = true;
            try { Text = e.NewTextValue; }
            finally { _suppressTextPropertyReaction = false; }
        }

        TextChanged?.Invoke(this, e);

        if (!UseFloatingPlaceholder)
            return;

        if (!string.IsNullOrEmpty(e.NewTextValue) && string.IsNullOrEmpty(e.OldTextValue))
            _ = AnimatePlaceholderUp();
        else if (string.IsNullOrEmpty(e.NewTextValue))
            _ = AnimatePlaceholderDown();
    }

    // -----------------------
    // Placeholder move
    // -----------------------

    private void ApplyPlaceholderInstant()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        if (!UseFloatingPlaceholder)
        {
            FloatingPlaceholderLabel.IsVisible = false;
            InnerEditor.TranslationY = 0;
            return;
        }

        FloatingPlaceholderLabel.IsVisible = true;

        bool hasText = !string.IsNullOrEmpty(Text);

        if (!hasText)
        {
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = 0;
            FloatingPlaceholderLabel.Scale = 1;

            InnerEditor.TranslationY = 0;
        }
        else
        {
            FloatingPlaceholderLabel.TranslationX = 0;
            FloatingPlaceholderLabel.TranslationY = PlaceholderUpY;
            FloatingPlaceholderLabel.Scale = PlaceholderScale;

            InnerEditor.TranslationY = EditorDownY;
        }
    }

    private Task AnimatePlaceholderUp()
    {
        StopPlaceholderAnimations();
        EnsurePlaceholderAnchor();

        var tcs = new TaskCompletionSource();

        var startPH_Y = FloatingPlaceholderLabel.TranslationY;
        var startPH_S = FloatingPlaceholderLabel.Scale;
        var startEdY = InnerEditor.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (PlaceholderUpY - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (PlaceholderScale - startPH_S) * progress;

                InnerEditor.TranslationY =
                    startEdY + (EditorDownY - startEdY) * progress;
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
        var startEdY = InnerEditor.TranslationY;

        this.Animate(
            name: PlaceholderAnimName,
            callback: progress =>
            {
                FloatingPlaceholderLabel.TranslationX = 0;
                FloatingPlaceholderLabel.TranslationY =
                    startPH_Y + (0 - startPH_Y) * progress;

                FloatingPlaceholderLabel.Scale =
                    startPH_S + (1 - startPH_S) * progress;

                InnerEditor.TranslationY =
                    startEdY + (0 - startEdY) * progress;
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
    }

    // -----------------------
    // Focus & tap
    // -----------------------

    private void OnInnerEditorFocused(object? sender, FocusEventArgs e)
        => EditorFocused?.Invoke(this, e);

    private void OnInnerEditorUnfocused(object? sender, FocusEventArgs e)
        => EditorFocused?.Invoke(this, e);

    private void OnReadOnlyTapped(object? sender, EventArgs e)
    {
        if (!IsReadOnly || !RaiseTapWhenReadOnly) return;
        ReadOnlyTapped?.Invoke(this, EventArgs.Empty);
    }

    private async void OnTapCatcherTapped(object? sender, EventArgs e)
    {
        if (!InnerEditor.IsEnabled || !InnerEditor.IsVisible)
            return;

        if (IsReadOnly)
        {
            if (RaiseTapWhenReadOnly)
                ReadOnlyTapped?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (InnerEditor.IsFocused)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            TapCatcher.IsEnabled = false;
            TapCatcher.IsEnabled = true;

            InnerEditor.Focus();
            await Task.Delay(10);
            InnerEditor.Focus();
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

        if (propertyName == nameof(CornerRadius) ||
            propertyName == BorderThicknessProperty.PropertyName)
        {
            ApplyErrorBorderVisualState();
        }
    }

}
