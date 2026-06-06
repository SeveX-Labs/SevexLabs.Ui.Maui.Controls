using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialScrollViewTests : ContentPage
{
    private readonly List<string> _logEntries = new();

    public MaterialScrollViewTests()
    {
        InitializeComponent();
        AddLog("Pagina caricata");
    }

    private void OnScrollEnabledToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is not Switch toggle || string.IsNullOrWhiteSpace(toggle.StyleId))
        {
            return;
        }

        var scrollView = this.FindByName<MaterialScrollView>(toggle.StyleId);
        if (scrollView is null)
        {
            AddLog($"Toggle senza target: {toggle.StyleId}");
            return;
        }

        scrollView.IsScrollEnabled = e.Value;
        AddLog($"{toggle.StyleId}: IsScrollEnabled={e.Value}");
    }

    private void OnMaterialButtonClicked(object? sender, TappedEventArgs e)
    {
        if (sender is MaterialButton button)
        {
            AddLog($"Tap MaterialButton: {button.Text}");
        }
    }

    private void OnLabelTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            AddLog($"Tap Label: {label.Text}");
        }
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            AddLog($"Focus Entry: {entry.Placeholder}");
        }
    }

    private void OnChildSwitchToggled(object? sender, ToggledEventArgs e)
    {
        AddLog($"Switch child: {e.Value}");
    }

    private void OnChildCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        AddLog($"CheckBox child: {e.Value}");
    }

    private void AddLog(string message)
    {
        _logEntries.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");

        if (_logEntries.Count > 6)
        {
            _logEntries.RemoveRange(6, _logEntries.Count - 6);
        }

        LogLabel.Text = string.Join(Environment.NewLine, _logEntries);
    }
}
