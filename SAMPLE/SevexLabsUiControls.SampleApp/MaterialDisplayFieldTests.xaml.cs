using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialDisplayFieldTests : ContentPage
{
    private int _tapCount;

    public MaterialDisplayFieldTests()
    {
        InitializeComponent();
    }

    private void OnDisplayFieldTapped(object? sender, TappedEventArgs e)
    {
        _tapCount++;

        if (sender is MaterialDisplayField field)
        {
            ResultLabel.Text = $"Tap #{_tapCount}: {field.Placeholder} - {field.Text}";
            return;
        }

        ResultLabel.Text = $"Tap #{_tapCount}";
    }

    private void OnToggleErrorFieldTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialDisplayField field)
        {
            return;
        }

        field.HasError = !field.HasError;

        field.Text = field.HasError
            ? "Errore attivo"
            : "Errore disattivato";

        ResultLabel.Text = field.HasError
            ? "HasError=True applicato al campo tappabile."
            : "HasError=False applicato al campo tappabile.";
    }

    private void OnStyledToggleErrorFieldTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialDisplayField field)
        {
            return;
        }

        field.HasError = !field.HasError;

        field.Text = field.HasError
            ? "Errore attivo"
            : "Errore disattivato";

        ResultLabel.Text = field.HasError
            ? "HasError=True applicato al campo con Style DefaultDisplayField."
            : "HasError=False applicato al campo con Style DefaultDisplayField.";
    }
    
    private void OnToggleDisplayFieldErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorDisplayField.HasError = !ButtonToggleErrorDisplayField.HasError;

        ButtonToggleErrorDisplayField.Text = ButtonToggleErrorDisplayField.HasError
            ? "Valore non valido"
            : "Valore valido";

        ResultLabel.Text = ButtonToggleErrorDisplayField.HasError
            ? "HasError=True applicato al MaterialDisplayField tramite bottone."
            : "HasError=False applicato al MaterialDisplayField tramite bottone.";
    }

    private void OnResetDisplayFieldErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorDisplayField.HasError = false;
        ButtonToggleErrorDisplayField.Text = "Valore valido";

        ResultLabel.Text = "MaterialDisplayField ripristinato tramite bottone.";
    }
}