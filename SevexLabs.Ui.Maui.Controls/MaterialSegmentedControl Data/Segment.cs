using PropertyChanged;

namespace SevexLabs.Ui.Maui.Controls;

[AddINotifyPropertyChangedInterface]
public class Segment
{
    #region auto-properties

    public string Id { get; }

    public string Label { get; }

    public bool IsSelected { get; private set; }

    public object? Tag { get; }

    #endregion

    #region ctor(s)

    public Segment(string id, string label, object? tag)
    {
        Id = id;
        Label = label;
        Tag = tag;
    }

    #endregion

    #region access methods

    public void WithIsSelected(bool isSelected)
    {
        IsSelected = isSelected;
    }

    #endregion
}