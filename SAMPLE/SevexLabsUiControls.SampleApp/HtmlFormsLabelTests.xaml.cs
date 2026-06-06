namespace SevexLabsUiControls.SampleApp;

public partial class HtmlFormsLabelTests : ContentPage
{
    private int _linkTapCount;

    public HtmlFormsLabelTests()
    {
        InitializeComponent();

        LinkColorLabel.Configure(OnHtmlLinkTappedAsync);
        LinkStyleLabel.Configure(OnHtmlLinkTappedAsync);
    }

    private Task OnHtmlLinkTappedAsync(string link)
    {
        _linkTapCount++;
        ResultLabel.Text = $"Link tapped #{_linkTapCount}: {link}";

        return Task.CompletedTask;
    }
}
