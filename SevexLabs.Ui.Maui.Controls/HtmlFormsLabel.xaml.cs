using HtmlAgilityPack;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Label that converts a small supported HTML subset into a formatted MAUI label.
/// </summary>
/// <remarks>
/// This is not a complete HTML parser. Supported tags are a, b, strong, i, em, u, and br.
/// </remarks>
public partial class HtmlFormsLabel
{

    #region nested classes

    private class ParsedElement
    {
        #region auto-properties

        public bool IsTag { get; set; }
        public bool IsLink { get; set; }

        public string? Text { get; set; }
        public FontAttributes ExpectedFontAttribute { get; set; }
        public TextDecorations ExpectedTextDecoration { get; set; }

        public string? Link { get; set; }

        #endregion
    }

    private class HtmlFormsSpan : Span
    {
        #region bindable properties

        public static readonly BindableProperty LinkProperty = BindableProperty.Create(nameof(Link), typeof(string), typeof(HtmlFormsSpan), defaultValue: string.Empty, propertyChanged: null);

        #endregion

        #region properties

        public string Link
        {
            get => (string)GetValue(LinkProperty);
            set => SetValue(LinkProperty, value);
        }

        #endregion
    }

    #endregion

    #region const

    private static readonly string[] AllowedTagNames = { "a", "b", "i", "u", "em", "strong", "br" };

    #endregion

    #region bindable properties

    public static readonly BindableProperty HtmlTextProperty = BindableProperty.Create(nameof(HtmlText), typeof(string), typeof(HtmlFormsLabel), defaultValue: string.Empty, propertyChanged: (bindable, _, newVal) =>
    {
        var htmlLabel = (HtmlFormsLabel)bindable;
        htmlLabel.FormattedText?.Spans?.Clear();

        string? newText = newVal as string;
        if (!string.IsNullOrEmpty(newText))
        {
            List<ParsedElement> parsedElements = htmlLabel.ParseText(newText);
            if (parsedElements.Any())
            {
                FormattedString? fs = htmlLabel.MakeFormattedStringFromParsedElements(parsedElements);
                if (fs is not null) htmlLabel.FormattedText = fs;
            }
        }
    });

    public static readonly BindableProperty LinkStyleProperty = BindableProperty.Create(nameof(LinkStyle), typeof(Style), typeof(HtmlFormsLabel), defaultValue: null, propertyChanged: null);
    public static readonly BindableProperty LinkColorProperty = BindableProperty.Create(nameof(LinkColor), typeof(Color), typeof(HtmlFormsLabel), defaultValue: Color.FromArgb("#3FA8F4"), propertyChanged: null);

    public static readonly BindableProperty HtmlFontFamilyProperty = BindableProperty.Create(nameof(HtmlFontFamily), typeof(string), typeof(HtmlFormsLabel), defaultValue: string.Empty, propertyChanged: null);
    public static readonly BindableProperty HtmlFontBoldFamilyProperty = BindableProperty.Create(nameof(HtmlFontBoldFamily), typeof(string), typeof(HtmlFormsLabel), defaultValue: string.Empty, propertyChanged: null);
    public static readonly BindableProperty HtmlFontItalicFamilyProperty = BindableProperty.Create(nameof(HtmlFontItalicFamily), typeof(string), typeof(HtmlFormsLabel), defaultValue: string.Empty, propertyChanged: null);

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets the HTML-like text converted into <see cref="Label.FormattedText"/>.
    /// </summary>
    /// <remarks>
    /// Only a, b, strong, i, em, u, and br are handled.
    /// </remarks>
    public string HtmlText
    {
        get => (string)GetValue(HtmlTextProperty);
        set => SetValue(HtmlTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the style applied to link spans.
    /// </summary>
    public Style LinkStyle
    {
        get => (Style)GetValue(LinkStyleProperty);
        set => SetValue(LinkStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the color applied to generated link spans.
    /// </summary>
    public Color LinkColor
    {
        get => (Color)GetValue(LinkColorProperty);
        set => SetValue(LinkColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family used by generated spans when provided.
    /// </summary>
    public string HtmlFontFamily
    {
        get => (string)GetValue(HtmlFontFamilyProperty);
        set => SetValue(HtmlFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family used for generated bold spans when provided.
    /// </summary>
    public string HtmlFontBoldFamily
    {
        get => (string)GetValue(HtmlFontBoldFamilyProperty);
        set => SetValue(HtmlFontBoldFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family used for generated italic spans when provided.
    /// </summary>
    public string HtmlFontItalicFamily
    {
        get => (string)GetValue(HtmlFontItalicFamilyProperty);
        set => SetValue(HtmlFontItalicFamilyProperty, value);
    }

    private Func<string, Task>? ActOnLinkTapped;

    #endregion

    #region ctor(s)

    public HtmlFormsLabel()
    {
        InitializeComponent();

        this.Text = string.Empty;
    }

    #endregion


    #region access methods

    /// <summary>
    /// Configures the callback invoked when a generated link span is tapped.
    /// </summary>
    public void Configure(Func<string, Task> actOnLinkTapped)
    {
        ActOnLinkTapped = actOnLinkTapped;
    }

    #endregion


    #region helper method

    private List<ParsedElement> ParseText(string htmlText)
    {
        HtmlDocument html = new HtmlDocument();
        html.LoadHtml(htmlText);
        var descendants = html.DocumentNode.Descendants();
        List<ParsedElement> parsedElements = new List<ParsedElement>();
        ParsedElement? parsedElement = null;
        foreach (var node in descendants)
        {
            parsedElement ??= new ParsedElement();
            if (node.NodeType == HtmlNodeType.Text)
            {
                parsedElement.Text += node.InnerText;
                parsedElements.Add(parsedElement);
                parsedElement = null;
            }
            if (node.NodeType == HtmlNodeType.Element && parsedElement is not null)
            {
                if (AllowedTagNames.Contains(node.Name))
                {
                    parsedElement.IsTag = true;
                    switch (node.Name)
                    {
                        case "br":
                            parsedElement.Text += "\r\n";
                            break;
                        case "a":
                            parsedElement.ExpectedTextDecoration = TextDecorations.Underline;
                            parsedElement.IsLink = true;
                            if (node.HasAttributes)
                            {
                                var hrefAttribute = node.Attributes.FirstOrDefault(a => a.Name == "href");
                                if (!(hrefAttribute is null))
                                    parsedElement.Link = hrefAttribute.Value;
                            }
                            break;
                        case "b":
                        case "strong":
                            parsedElement.ExpectedFontAttribute = FontAttributes.Bold;
                            break;
                        case "i":
                        case "em":
                            parsedElement.ExpectedFontAttribute = FontAttributes.Italic;
                            break;
                        case "u":
                            parsedElement.ExpectedTextDecoration = TextDecorations.Underline;
                            break;
                    }
                }
            }
        }
        return parsedElements;
    }

    private FormattedString? MakeFormattedStringFromParsedElements(List<ParsedElement>? parsedElements)
    {
        FormattedString? fs = null;
        if (!(parsedElements is null))
        {
            fs = new FormattedString();
            foreach (var parsedElement in parsedElements)
            {
                HtmlFormsSpan span = new HtmlFormsSpan();
                span.Style = this.Style;
                span.LineHeight = this.LineHeight;
                span.FontSize = this.FontSize;
                span.FontFamily = (!string.IsNullOrEmpty(this.HtmlFontFamily)) ? this.HtmlFontFamily : this.FontFamily;

                if (!string.IsNullOrEmpty(parsedElement.Text))
                {
                    span.Text = parsedElement.Text;
                    if (parsedElement.IsLink || parsedElement.IsTag)
                    {
                        // span.FontAttributes = parsedElement.ExpectedFontAttribute;
                        switch (parsedElement.ExpectedFontAttribute)
                        {
                            case FontAttributes.Bold:
                                span.FontAttributes = FontAttributes.Bold;
                                if (!string.IsNullOrEmpty(this.HtmlFontBoldFamily))
                                    span.FontFamily = this.HtmlFontBoldFamily;
                                break;
                            case FontAttributes.Italic:
                                span.FontAttributes = FontAttributes.Italic;
                                if (!string.IsNullOrEmpty(this.HtmlFontItalicFamily))
                                    span.FontFamily = this.HtmlFontItalicFamily;
                                break;
                        }
                        span.TextDecorations = parsedElement.ExpectedTextDecoration;

                        if (parsedElement.IsLink)
                        {
                            var spanTapGesture = new TapGestureRecognizer();
                            if (!string.IsNullOrEmpty(parsedElement.Link))
                            {
                                spanTapGesture.Tapped += async (_, _) =>
                                {
                                    if (!(ActOnLinkTapped is null))
                                        await ActOnLinkTapped(parsedElement.Link);
                                    else
                                        await Browser.OpenAsync(parsedElement.Link, BrowserLaunchMode.SystemPreferred);
                                };
                                span.Link = parsedElement.Link;
                            }

                            bool hasStyleLinkColor = false;
                            if (!(LinkStyle is null))
                            {
                                hasStyleLinkColor = LinkStyle.Setters.Any(setter => setter.Property == TextColorProperty);
                                var fontFamilySetter = LinkStyle.Setters.FirstOrDefault(setter => setter.Property == FontFamilyProperty);
                                if (!(fontFamilySetter is null)) span.FontFamily = (OnPlatform<string>)fontFamilySetter.Value;
                                span.Style = LinkStyle;
                            }
                            if (!hasStyleLinkColor) span.TextColor = LinkColor;

                            span.GestureRecognizers.Add(spanTapGesture);
                        }
                    }
                    fs.Spans.Add(span);
                }
            }
        }
        return fs;
    }

    #endregion
}
