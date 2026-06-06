using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialEditorTests : ContentPage
{
    private int _textChangedCount;
    private int _readOnlyTapCount;
    private int _focusEventCount;

    public MaterialEditorTests()
    {
        InitializeComponent();
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        _textChangedCount++;

        ResultLabel.Text = $"TextChanged #{_textChangedCount}: {e.NewTextValue}";
    }

    private void OnEditorFocused(object? sender, FocusEventArgs e)
    {
        _focusEventCount++;

        ResultLabel.Text = e.IsFocused
            ? $"EditorFocused #{_focusEventCount}: focused"
            : $"EditorFocused #{_focusEventCount}: unfocused";
    }

    private void OnReadOnlyEditorTapped(object? sender, EventArgs e)
    {
        _readOnlyTapCount++;

        ResultLabel.Text = $"ReadOnlyTapped #{_readOnlyTapCount}";
    }

    private void OnToggleErrorEditorTapped(object? sender, EventArgs e)
    {
        if (sender is not MaterialEditor editor)
        {
            return;
        }

        editor.HasError = !editor.HasError;

        editor.Text = editor.HasError
            ? "Errore attivo"
            : "Errore disattivato";

        ResultLabel.Text = editor.HasError
            ? "HasError=True applicato al MaterialEditor tappabile."
            : "HasError=False applicato al MaterialEditor tappabile.";
    }

    private void OnStyledToggleErrorEditorTapped(object? sender, EventArgs e)
    {
        if (sender is not MaterialEditor editor)
        {
            return;
        }

        editor.HasError = !editor.HasError;

        editor.Text = editor.HasError
            ? "Errore attivo con DefaultEditor"
            : "Errore disattivato con DefaultEditor";

        ResultLabel.Text = editor.HasError
            ? "HasError=True applicato al MaterialEditor con Style DefaultEditor."
            : "HasError=False applicato al MaterialEditor con Style DefaultEditor.";
    }
    
    private void OnToggleEditorErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorEditor.HasError = !ButtonToggleErrorEditor.HasError;

        ButtonToggleErrorEditor.Text = ButtonToggleErrorEditor.HasError
            ? "Testo non valido"
            : "Testo valido";

        ResultLabel.Text = ButtonToggleErrorEditor.HasError
            ? "HasError=True applicato al MaterialEditor tramite bottone."
            : "HasError=False applicato al MaterialEditor tramite bottone.";
    }

    private void OnResetEditorErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorEditor.HasError = false;
        ButtonToggleErrorEditor.Text = "Testo valido";

        ResultLabel.Text = "MaterialEditor ripristinato tramite bottone.";
    }
}