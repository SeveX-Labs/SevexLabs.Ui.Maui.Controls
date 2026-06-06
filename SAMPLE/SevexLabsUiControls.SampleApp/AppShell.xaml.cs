namespace SevexLabsUiControls.SampleApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(ControlsGalleryPage), typeof(ControlsGalleryPage));
        Routing.RegisterRoute(nameof(FastBorderTests), typeof(FastBorderTests));
        Routing.RegisterRoute(nameof(BorderVsFastBorderProfilingTests), typeof(BorderVsFastBorderProfilingTests));
        Routing.RegisterRoute(nameof(MaterialButtonTests), typeof(MaterialButtonTests));
        Routing.RegisterRoute(nameof(MaterialDisplayFieldTests), typeof(MaterialDisplayFieldTests));
        Routing.RegisterRoute(nameof(MaterialEditorTests), typeof(MaterialEditorTests));
        Routing.RegisterRoute(nameof(MaterialEntryTests), typeof(MaterialEntryTests));
        Routing.RegisterRoute(nameof(MaterialErrorStateTests), typeof(MaterialErrorStateTests));
        Routing.RegisterRoute(nameof(MaterialNumericEntryTests), typeof(MaterialNumericEntryTests));
        Routing.RegisterRoute(nameof(MaterialPickerTests), typeof(MaterialPickerTests));
        Routing.RegisterRoute(nameof(MaterialSegmentedControlTests), typeof(MaterialSegmentedControlTests));
        Routing.RegisterRoute(nameof(MaterialFlexChipsTests), typeof(MaterialFlexChipsTests));
        Routing.RegisterRoute(nameof(MaterialScrollViewTests), typeof(MaterialScrollViewTests));
        Routing.RegisterRoute(nameof(HtmlFormsLabelTests), typeof(HtmlFormsLabelTests));
        Routing.RegisterRoute(nameof(SlideStepsViewTests), typeof(SlideStepsViewTests));
        Routing.RegisterRoute(nameof(MediaProgressBarTests), typeof(MediaProgressBarTests));
        Routing.RegisterRoute(nameof(CountdownControlsTests), typeof(CountdownControlsTests));
        Routing.RegisterRoute(nameof(LifecycleStressTests), typeof(LifecycleStressTests));
        Routing.RegisterRoute(nameof(PrintPage), typeof(PrintPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
