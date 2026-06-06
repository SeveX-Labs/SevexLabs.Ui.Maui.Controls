namespace SevexLabs.Ui.Maui.Controls;

public partial class BorderlessEditor
{
    #region bindable properties

    public static readonly BindableProperty PlaceholderFontFamilyProperty = BindableProperty.Create(nameof(PlaceholderFontFamily), typeof(string), typeof(BorderlessEditor), defaultValue: null);

    #endregion

    #region properties

    public string PlaceholderFontFamily
    {
        get => (string)GetValue(PlaceholderFontFamilyProperty);
        set => SetValue(PlaceholderFontFamilyProperty, value);
    }

    #endregion

    #region ctor(s)

    public BorderlessEditor()
    {
        InitializeComponent();
    }

    #endregion
}
