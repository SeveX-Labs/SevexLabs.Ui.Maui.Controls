using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialEntryTests : ContentPage
{
    private int _textChangedCount;
    private int _focusEventCount;
    private int _readOnlyTapCount;

    public MaterialEntryTests()
    {
        InitializeComponent();
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        _textChangedCount++;

        ResultLabel.Text = $"TextChanged #{_textChangedCount}: {e.NewTextValue}";
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        _focusEventCount++;

        ResultLabel.Text = e.IsFocused
            ? $"EntryFocused #{_focusEventCount}: focused"
            : $"EntryFocused #{_focusEventCount}: unfocused";
    }

    private void OnReadOnlyEntryTapped(object? sender, TappedEventArgs e)
    {
        _readOnlyTapCount++;

        ResultLabel.Text = $"ReadOnlyTapped #{_readOnlyTapCount}";
    }

    private void OnToggleErrorEntryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialEntry entry)
        {
            return;
        }

        entry.HasError = !entry.HasError;

        entry.Text = entry.HasError
            ? "Errore attivo"
            : "Errore disattivato";

        ResultLabel.Text = entry.HasError
            ? "HasError=True applicato al MaterialEntry tappabile."
            : "HasError=False applicato al MaterialEntry tappabile.";
    }

    private void OnStyledToggleErrorEntryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialEntry entry)
        {
            return;
        }

        entry.HasError = !entry.HasError;

        entry.Text = entry.HasError
            ? "Errore attivo con DefaultEntry"
            : "Errore disattivato con DefaultEntry";

        ResultLabel.Text = entry.HasError
            ? "HasError=True applicato al MaterialEntry con Style DefaultEntry."
            : "HasError=False applicato al MaterialEntry con Style DefaultEntry.";
    }
    
    private void OnToggleEntryErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorEntry.HasError = !ButtonToggleErrorEntry.HasError;

        ButtonToggleErrorEntry.Text = ButtonToggleErrorEntry.HasError
            ? "Valore non valido"
            : "Valore valido";

        ResultLabel.Text = ButtonToggleErrorEntry.HasError
            ? "HasError=True applicato al MaterialEntry tramite bottone."
            : "HasError=False applicato al MaterialEntry tramite bottone.";
    }

    private void OnResetEntryErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorEntry.HasError = false;
        ButtonToggleErrorEntry.Text = "Valore valido";

        ResultLabel.Text = "MaterialEntry ripristinato tramite bottone.";
    }
}