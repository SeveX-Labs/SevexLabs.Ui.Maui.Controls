using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialButtonTests : ContentPage
{
    private bool _isToggleLoadingActive;

    public MaterialButtonTests()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object? sender, TappedEventArgs e)
    {
        if (sender is MaterialButton button)
        {
            ResultLabel.Text = $"Click: {button.Text}";
        }
        else
        {
            ResultLabel.Text = "Click ricevuto.";
        }
    }

    private void OnToggleLoadingClicked(object? sender, TappedEventArgs e)
    {
        _isToggleLoadingActive = !_isToggleLoadingActive;

        ToggleLoadingButton.IsLoading = _isToggleLoadingActive;
        ToggleLoadingButton.Text = _isToggleLoadingActive
            ? "Loading..."
            : "Vai al dettaglio esercizio";

        ResultLabel.Text = _isToggleLoadingActive
            ? "ToggleLoadingButton in loading."
            : "ToggleLoadingButton tornato normale.";
    }
}
