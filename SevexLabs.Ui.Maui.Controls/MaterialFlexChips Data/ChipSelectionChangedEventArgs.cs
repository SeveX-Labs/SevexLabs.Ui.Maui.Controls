namespace SevexLabs.Ui.Maui.Controls;

public class ChipSelectionChangedEventArgs : EventArgs
{
    public IEnumerable<object> SelectedItems { get; }

    public ChipSelectionChangedEventArgs(IEnumerable<object> selectedItems)
    {
        SelectedItems = selectedItems;
    }
}