namespace SevexLabsUiControls.SampleApp;

public partial class TestPagesMenuPage : ContentPage
{
    public TestPagesMenuPage()
    {
        InitializeComponent();
    }

    private async void OnControlsGalleryClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(ControlsGalleryPage));
    }

    private async void OnFastBorderTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(FastBorderTests));
    }

    private async void OnBorderVsFastBorderProfilingTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(BorderVsFastBorderProfilingTests));
    }

    private async void OnMaterialButtonTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialButtonTests));
    }

    private async void OnMaterialDisplayFieldTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialDisplayFieldTests));
    }

    private async void OnMaterialEditorTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialEditorTests));
    }

    private async void OnMaterialEntryTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialEntryTests));
    }

    private async void OnMaterialErrorStateTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialErrorStateTests));
    }

    private async void OnMaterialNumericEntryTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialNumericEntryTests));
    }

    private async void OnMaterialSegmentedControlTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialSegmentedControlTests));
    }

    private async void OnMaterialFlexChipsTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialFlexChipsTests));
    }

    private async void OnMaterialScrollViewTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialScrollViewTests));
    }

    private async void OnMaterialPickerTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MaterialPickerTests));
    }

    private async void OnHtmlFormsLabelTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(HtmlFormsLabelTests));
    }

    private async void OnSlideStepsViewTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(SlideStepsViewTests));
    }

    private async void OnMediaProgressBarTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(MediaProgressBarTests));
    }

    private async void OnCountdownControlsTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(CountdownControlsTests));
    }

    private async void OnLifecycleStressTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(LifecycleStressTests));
    }
    
    private async void OnPrintPageTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(PrintPage));
    }
    
    private async void OnSettingsPageTestsClicked(object? sender, TappedEventArgs e)
    {
        await GoToPage(nameof(SettingsPage));
    }

    private static async Task GoToPage(string route)
    {
        await Shell.Current.GoToAsync(route);
    }
}
