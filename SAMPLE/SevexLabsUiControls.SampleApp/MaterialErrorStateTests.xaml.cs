using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialErrorStateTests : ContentPage
{
    private bool _dynamicResourceAlt;
    private bool _inlineEntryAlt;
    private bool _inlineEditorAlt;
    private bool _inlineDisplayAlt;
    private bool _inlineNumericAlt;

    public MaterialErrorStateTests()
    {
        InitializeComponent();
        UpdateLog("Pagina caricata");
    }

    private void OnHasErrorToggled(object? sender, ToggledEventArgs e)
    {
        if (TryFindControl(sender, out var control))
        {
            SetHasError(control, e.Value);
            UpdateLog($"{control.StyleId ?? control.GetType().Name}: HasError={e.Value}");
        }
    }

    private void OnEnabledToggled(object? sender, ToggledEventArgs e)
    {
        if (TryFindControl(sender, out var control))
        {
            control.IsEnabled = e.Value;
            UpdateLog($"{control.StyleId ?? control.GetType().Name}: IsEnabled={e.Value}");
        }
    }

    private void OnChangeDynamicResourceClicked(object? sender, TappedEventArgs e)
    {
        _dynamicResourceAlt = !_dynamicResourceAlt;
        Resources["ErrorStateDynamicBorderColor"] = _dynamicResourceAlt
            ? Color.FromArgb("#16A34A")
            : Color.FromArgb("#2563EB");

        UpdateLog("DynamicResource ErrorStateDynamicBorderColor cambiata");
    }

    private void OnChangeInlineBorderClicked(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement element || string.IsNullOrWhiteSpace(element.StyleId))
        {
            return;
        }

        var useAlt = ToggleInlineState(element.StyleId);
        var control = this.FindByName<FastBorder>(element.StyleId);
        if (control is null)
        {
            return;
        }

        control.BorderColor = useAlt
            ? Color.FromArgb("#7C3AED")
            : Color.FromArgb("#D1D5DB");
        control.BorderThickness = useAlt ? 4 : 1;

        UpdateLog($"{element.StyleId}: BorderColor/BorderThickness cambiati a runtime");
    }

    private bool TryFindControl(object? sender, out FastBorder control)
    {
        control = null!;

        if (sender is not Switch toggle || string.IsNullOrWhiteSpace(toggle.StyleId))
        {
            return false;
        }

        control = this.FindByName<FastBorder>(toggle.StyleId);
        return control is not null;
    }

    private bool ToggleInlineState(string controlName)
    {
        return controlName switch
        {
            nameof(InlineEntry) => _inlineEntryAlt = !_inlineEntryAlt,
            nameof(InlineEditor) => _inlineEditorAlt = !_inlineEditorAlt,
            nameof(InlineDisplay) => _inlineDisplayAlt = !_inlineDisplayAlt,
            nameof(InlineNumeric) => _inlineNumericAlt = !_inlineNumericAlt,
            _ => false
        };
    }

    private static void SetHasError(FastBorder control, bool value)
    {
        switch (control)
        {
            case MaterialEntry entry:
                entry.HasError = value;
                break;
            case MaterialEditor editor:
                editor.HasError = value;
                break;
            case MaterialDisplayField display:
                display.HasError = value;
                break;
            case MaterialNumericEntry numeric:
                numeric.HasError = value;
                break;
        }
    }

    private static bool GetHasError(FastBorder control)
    {
        return control switch
        {
            MaterialEntry entry => entry.HasError,
            MaterialEditor editor => editor.HasError,
            MaterialDisplayField display => display.HasError,
            MaterialNumericEntry numeric => numeric.HasError,
            _ => false
        };
    }

    private void UpdateLog(string message)
    {
        if (LogLabel is null)
        {
            return;
        }

        LogLabel.Text =
            $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}" +
            FormatControl("Entry style", StyledEntry) + Environment.NewLine +
            FormatControl("Entry inline", InlineEntry) + Environment.NewLine +
            FormatControl("Entry dynamic", DynamicEntry) + Environment.NewLine +
            FormatControl("Editor style", StyledEditor) + Environment.NewLine +
            FormatControl("Editor inline", InlineEditor) + Environment.NewLine +
            FormatControl("Editor dynamic", DynamicEditor) + Environment.NewLine +
            FormatControl("Display style", StyledDisplay) + Environment.NewLine +
            FormatControl("Display inline", InlineDisplay) + Environment.NewLine +
            FormatControl("Display dynamic", DynamicDisplay) + Environment.NewLine +
            FormatControl("Numeric style", StyledNumeric) + Environment.NewLine +
            FormatControl("Numeric inline", InlineNumeric) + Environment.NewLine +
            FormatControl("Numeric dynamic", DynamicNumeric);
    }

    private static string FormatControl(string label, FastBorder control)
    {
        return $"{label}: BorderColor={control.BorderColor}, BorderThickness={control.BorderThickness}, HasError={GetHasError(control)}, IsEnabled={control.IsEnabled}";
    }
}
