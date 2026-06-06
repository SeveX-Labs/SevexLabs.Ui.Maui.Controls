namespace SevexLabs.Ui.Maui.Controls;

public class MaterialPicker : Picker
{
    public event EventHandler? SelectionConfirmed;
    public event EventHandler? SelectionCancelled;

    internal int? InitialSelectedIndex { get; set; }

    public void RaiseSelectionConfirmed()
    {
        SelectionConfirmed?.Invoke(this, EventArgs.Empty);
        InitialSelectedIndex = null; // clear
    }

    public void RaiseSelectionCancelled()
    {
        if (InitialSelectedIndex.HasValue)
            SelectedIndex = InitialSelectedIndex.Value;

        SelectionCancelled?.Invoke(this, EventArgs.Empty);
        InitialSelectedIndex = null;
    }
}