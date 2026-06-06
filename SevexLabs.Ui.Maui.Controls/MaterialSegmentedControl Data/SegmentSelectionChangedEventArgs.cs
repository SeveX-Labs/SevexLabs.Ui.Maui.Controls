namespace SevexLabs.Ui.Maui.Controls;

public class SegmentSelectionChangedEventArgs : EventArgs
{
    public Segment? SelectedSegment { get; }

    public SegmentSelectionChangedEventArgs(Segment? selectedSegment)
    {
        SelectedSegment = selectedSegment;
    }
}