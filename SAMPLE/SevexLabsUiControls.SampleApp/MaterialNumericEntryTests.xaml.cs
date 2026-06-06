using SevexLabs.Ui.Maui.Controls;
using SevexLabs.Ui.Maui.Controls.Model.Event_Args;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialNumericEntryTests : ContentPage
{
    private readonly MaterialNumericEntry _lifecycleNumericEntry;
    private int _valueChangedCount;
    private int _readOnlyTapCount;
    private int _manualInputValueChangedCount;
    private int _manualInputReadOnlyTapCount;
    private int _lifecycleMountCount;
    private int _lifecycleValueChangedCount;
    private int _lifecycleReadOnlyTapCount;
    private bool _suppressManualInputValueChanged;
    private bool _suppressLifecycleValueChanged;

    public MaterialNumericEntryTests()
    {
        InitializeComponent();

        _lifecycleNumericEntry = CreateLifecycleNumericEntry();
        MountLifecycleEntry("Lifecycle entry mounted.");
        UpdateManualInputStatus("Manual input disabled tests ready.");
    }

    private void OnNumericValueChanged(object? sender, NullableControlValueChangedEventArgs e)
    {
        _valueChangedCount++;

        var oldValue = e.OldValue?.ToString() ?? "null";
        var newValue = e.NewValue?.ToString() ?? "null";

        if (sender is MaterialNumericEntry numericEntry)
        {
            ResultLabel.Text = $"ValueChanged #{_valueChangedCount}: {numericEntry.Placeholder} | {oldValue} → {newValue}";
            return;
        }

        ResultLabel.Text = $"ValueChanged #{_valueChangedCount}: {oldValue} → {newValue}";
    }

    private void OnReadOnlyNumericEntryTapped(object? sender, TappedEventArgs e)
    {
        _readOnlyTapCount++;

        ResultLabel.Text = $"ReadOnlyTapped #{_readOnlyTapCount}";
    }

    private void OnToggleErrorNumericEntryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialNumericEntry numericEntry)
        {
            return;
        }

        numericEntry.HasError = !numericEntry.HasError;

        numericEntry.Value = numericEntry.HasError
            ? 99
            : 12;

        ResultLabel.Text = numericEntry.HasError
            ? "HasError=True applicato al MaterialNumericEntry tappabile."
            : "HasError=False applicato al MaterialNumericEntry tappabile.";
    }

    private void OnStyledToggleErrorNumericEntryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not MaterialNumericEntry numericEntry)
        {
            return;
        }

        numericEntry.HasError = !numericEntry.HasError;

        numericEntry.Value = numericEntry.HasError
            ? 19
            : 7;

        ResultLabel.Text = numericEntry.HasError
            ? "HasError=True applicato al MaterialNumericEntry con Style DefaultNumericEntry."
            : "HasError=False applicato al MaterialNumericEntry con Style DefaultNumericEntry.";
    }
    
    private void OnToggleNumericErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorNumericEntry.HasError = !ButtonToggleErrorNumericEntry.HasError;

        ButtonToggleErrorNumericEntry.Value = ButtonToggleErrorNumericEntry.HasError
            ? 101
            : 25;

        ResultLabel.Text = ButtonToggleErrorNumericEntry.HasError
            ? "HasError=True applicato tramite bottone."
            : "HasError=False applicato tramite bottone.";
    }

    private void OnResetNumericErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorNumericEntry.HasError = false;
        ButtonToggleErrorNumericEntry.Value = 25;

        ResultLabel.Text = "MaterialNumericEntry ripristinato tramite bottone.";
    }

    private void OnManualInputNumericValueChanged(object? sender, NullableControlValueChangedEventArgs e)
    {
        if (_suppressManualInputValueChanged)
        {
            return;
        }

        _manualInputValueChangedCount++;

        var label = sender is MaterialNumericEntry numericEntry
            ? numericEntry.Placeholder
            : "Manual input";

        UpdateManualInputStatus($"{label}: {FormatNullableValue(e.OldValue)} -> {FormatNullableValue(e.NewValue)}");
    }

    private void OnManualInputReadOnlyTapped(object? sender, TappedEventArgs e)
    {
        _manualInputReadOnlyTapCount++;

        var label = sender is MaterialNumericEntry numericEntry
            ? numericEntry.Placeholder
            : "Manual input";

        UpdateManualInputStatus($"ReadOnlyTapped: {label}");
    }

    private void OnManualInputToggleClicked(object? sender, TappedEventArgs e)
    {
        ManualInputToggleEntry.IsManualInputDisabled = !ManualInputToggleEntry.IsManualInputDisabled;

        UpdateManualInputStatus(ManualInputToggleEntry.IsManualInputDisabled
            ? "Runtime toggle manual input disabled."
            : "Runtime toggle manual input enabled.");
    }

    private void OnManualInputReadOnlyToggleClicked(object? sender, TappedEventArgs e)
    {
        ManualInputToggleEntry.IsReadOnly = !ManualInputToggleEntry.IsReadOnly;

        UpdateManualInputStatus(ManualInputToggleEntry.IsReadOnly
            ? "Runtime toggle read-only enabled."
            : "Runtime toggle read-only disabled.");
    }

    private void OnManualInputIncrementClicked(object? sender, TappedEventArgs e)
    {
        ManualInputToggleEntry.Value = (ManualInputToggleEntry.Value ?? 0) + 1;
    }

    private void OnManualInputResetClicked(object? sender, TappedEventArgs e)
    {
        _manualInputValueChangedCount = 0;
        _manualInputReadOnlyTapCount = 0;

        _suppressManualInputValueChanged = true;
        try
        {
            ManualInputToggleEntry.IsManualInputDisabled = true;
            ManualInputToggleEntry.IsReadOnly = false;
            ManualInputToggleEntry.Value = 30;
        }
        finally
        {
            _suppressManualInputValueChanged = false;
        }

        UpdateManualInputStatus("Manual input counters reset.");
    }

    private void UpdateManualInputStatus(string status)
    {
        ManualInputToggleStateLabel.Text = $"Manual disabled: {ManualInputToggleEntry.IsManualInputDisabled}";
        ManualInputReadOnlyStateLabel.Text = $"Read-only: {ManualInputToggleEntry.IsReadOnly}";
        ManualInputValueChangedCountLabel.Text = $"ValueChanged: {_manualInputValueChangedCount}";
        ManualInputReadOnlyTapCountLabel.Text = $"ReadOnlyTapped: {_manualInputReadOnlyTapCount}";
        ManualInputCurrentValueLabel.Text = $"Runtime value: {FormatNullableValue(ManualInputToggleEntry.Value)}";
        ManualInputStatusLabel.Text = status;

        ResultLabel.Text = status;
    }

    private MaterialNumericEntry CreateLifecycleNumericEntry()
    {
        var numericEntry = new MaterialNumericEntry
        {
            HeightRequest = 56,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromArgb("#6366F1"),
            BorderThickness = 1,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(0),
            Placeholder = "Lifecycle read-only",
            Value = 10,
            Minimum = 0,
            Maximum = 100,
            Step = 1,
            DecimalPlaces = 0,
            UseFloatingPlaceholder = true,
            PlaceholderUpY = -10,
            PlaceholderScale = 0.9,
            EntryDownY = 10,
            FontSize = 16,
            TextColor = Colors.Black,
            PlaceholderColor = Color.FromArgb("#4F46E5"),
            ButtonsTextColor = Color.FromArgb("#4F46E5"),
            ButtonsFontSize = 28,
            IsReadOnly = true,
            RaiseTapWhenReadOnly = true
        };

        numericEntry.ValueChanged += OnLifecycleNumericValueChanged;
        numericEntry.ReadOnlyTapped += OnLifecycleNumericReadOnlyTapped;

        return numericEntry;
    }

    private void OnLifecycleMountClicked(object? sender, TappedEventArgs e)
    {
        MountLifecycleEntry("Lifecycle entry mounted.");
    }

    private void OnLifecycleUnmountClicked(object? sender, TappedEventArgs e)
    {
        UnmountLifecycleEntry("Lifecycle entry unmounted.");
    }

    private void OnLifecycleRemountClicked(object? sender, TappedEventArgs e)
    {
        UnmountLifecycleEntry();
        MountLifecycleEntry("Same lifecycle entry remounted.");
    }

    private void OnLifecycleCycleClicked(object? sender, TappedEventArgs e)
    {
        for (var i = 0; i < 10; i++)
        {
            UnmountLifecycleEntry();
            MountLifecycleEntry();
        }

        UpdateLifecycleStatus("Lifecycle entry cycled 10 times.");
    }

    private void OnLifecycleIncrementValueClicked(object? sender, TappedEventArgs e)
    {
        _lifecycleNumericEntry.Value = (_lifecycleNumericEntry.Value ?? 0) + 1;
    }

    private void OnLifecycleResetClicked(object? sender, TappedEventArgs e)
    {
        _lifecycleMountCount = 0;
        _lifecycleValueChangedCount = 0;
        _lifecycleReadOnlyTapCount = 0;

        _suppressLifecycleValueChanged = true;
        try
        {
            _lifecycleNumericEntry.Value = 10;
        }
        finally
        {
            _suppressLifecycleValueChanged = false;
        }

        UpdateLifecycleStatus("Lifecycle counters reset.");
    }

    private void OnLifecycleNumericValueChanged(object? sender, NullableControlValueChangedEventArgs e)
    {
        if (_suppressLifecycleValueChanged)
        {
            return;
        }

        _lifecycleValueChangedCount++;
        UpdateLifecycleStatus($"Lifecycle ValueChanged #{_lifecycleValueChangedCount}: {FormatNullableValue(e.OldValue)} -> {FormatNullableValue(e.NewValue)}");
    }

    private void OnLifecycleNumericReadOnlyTapped(object? sender, TappedEventArgs e)
    {
        _lifecycleReadOnlyTapCount++;
        UpdateLifecycleStatus($"Lifecycle ReadOnlyTapped #{_lifecycleReadOnlyTapCount}");
    }

    private void MountLifecycleEntry(string? status = null)
    {
        if (LifecycleHost.Content == _lifecycleNumericEntry)
        {
            UpdateLifecycleStatus(status ?? "Lifecycle entry already mounted.");
            return;
        }

        LifecycleHost.Content = _lifecycleNumericEntry;
        _lifecycleMountCount++;

        UpdateLifecycleStatus(status ?? "Lifecycle entry mounted.");
    }

    private void UnmountLifecycleEntry(string? status = null)
    {
        if (LifecycleHost.Content != _lifecycleNumericEntry)
        {
            if (status is not null)
            {
                UpdateLifecycleStatus(status);
            }

            return;
        }

        LifecycleHost.Content = null;

        if (status is not null)
        {
            UpdateLifecycleStatus(status);
        }
    }

    private void UpdateLifecycleStatus(string status)
    {
        LifecycleMountCountLabel.Text = $"Mount cycles: {_lifecycleMountCount}";
        LifecycleValueChangedCountLabel.Text = $"ValueChanged: {_lifecycleValueChangedCount}";
        LifecycleReadOnlyTapCountLabel.Text = $"ReadOnlyTapped: {_lifecycleReadOnlyTapCount}";
        LifecycleCurrentValueLabel.Text = $"Value: {FormatNullableValue(_lifecycleNumericEntry.Value)}";
        LifecycleStatusLabel.Text = status;

        ResultLabel.Text = status;
    }

    private static string FormatNullableValue(double? value)
    {
        return value?.ToString() ?? "null";
    }
}
