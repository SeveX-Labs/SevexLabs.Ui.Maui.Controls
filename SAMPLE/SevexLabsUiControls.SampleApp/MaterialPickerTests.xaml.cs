using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialPickerTests : ContentPage
{
    private int _boundSelectedIndex = 2;
    private string? _boundSelectedItem;
    private int _selectionChangedCount;
    private int _selectionConfirmedCount;
    private int _selectionCancelledCount;

    public MaterialPickerTests()
    {
        InitializeComponent();

        BindingContext = this;
    }

    public IReadOnlyList<string> VisitTypes { get; } = new[]
    {
        "Prima visita",
        "Controllo",
        "Dimissione",
        "Teleconsulto"
    };

    public int BoundSelectedIndex
    {
        get => _boundSelectedIndex;
        set
        {
            if (_boundSelectedIndex == value)
            {
                return;
            }

            _boundSelectedIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoundSummary));
        }
    }

    public string? BoundSelectedItem
    {
        get => _boundSelectedItem;
        set
        {
            if (_boundSelectedItem == value)
            {
                return;
            }

            _boundSelectedItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoundSummary));
        }
    }

    public string BoundSummary =>
        $"Bound SelectedIndex: {BoundSelectedIndex} - Bound SelectedItem: {BoundSelectedItem ?? "(null)"}";

    private void OnPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        _selectionChangedCount++;

        ResultLabel.Text = sender is Picker picker
            ? $"SelectedIndexChanged #{_selectionChangedCount}: {DescribePicker(picker)}"
            : $"SelectedIndexChanged #{_selectionChangedCount}";
    }

    private void OnPickerSelectionConfirmed(object? sender, EventArgs e)
    {
        _selectionConfirmedCount++;

        ResultLabel.Text = sender is Picker picker
            ? $"SelectionConfirmed #{_selectionConfirmedCount}: {DescribePicker(picker)}"
            : $"SelectionConfirmed #{_selectionConfirmedCount}";
    }

    private void OnPickerSelectionCancelled(object? sender, EventArgs e)
    {
        _selectionCancelledCount++;

        ResultLabel.Text = sender is Picker picker
            ? $"SelectionCancelled #{_selectionCancelledCount}: {DescribePicker(picker)}"
            : $"SelectionCancelled #{_selectionCancelledCount}";
    }

    private void OnSetBoundIndexZeroClicked(object? sender, TappedEventArgs e)
    {
        BoundSelectedIndex = 0;
        ResultLabel.Text = $"Binding request: BoundSelectedIndex=0. {BoundSummary}";
    }

    private void OnSetBoundIndexTwoClicked(object? sender, TappedEventArgs e)
    {
        BoundSelectedIndex = 2;
        ResultLabel.Text = $"Binding request: BoundSelectedIndex=2. {BoundSummary}";
    }

    private static string DescribePicker(Picker picker) =>
        $"SelectedIndex={picker.SelectedIndex}, SelectedItem={picker.SelectedItem ?? "(null)"}";
}
