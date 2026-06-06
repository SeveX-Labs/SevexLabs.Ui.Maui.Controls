namespace SevexLabsUiControls.SampleApp;

public partial class SlideStepsViewTests : ContentPage
{
    private int _currentIndex;
    private int _currentIndexChangedCount;
    private int _stepActionCount;

    public SlideStepsViewTests()
    {
        InitializeComponent();

        BindingContext = this;
    }

    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (_currentIndex == value)
            {
                return;
            }

            _currentIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentStepText));
        }
    }

    public string CurrentStepText => $"CurrentIndex: {CurrentIndex}";

    private async void OnPreviousClicked(object? sender, TappedEventArgs e)
    {
        await NavigateWithLogAsync("PreviousAsync", () => StepsView.PreviousAsync());
    }

    private async void OnNextClicked(object? sender, TappedEventArgs e)
    {
        await NavigateWithLogAsync("NextAsync", () => StepsView.NextAsync());
    }

    private void OnSetIndexZeroClicked(object? sender, TappedEventArgs e)
    {
        SetCurrentIndexFromButton(0);
    }

    private void OnSetIndexOneClicked(object? sender, TappedEventArgs e)
    {
        SetCurrentIndexFromButton(1);
    }

    private void OnSetIndexTwoClicked(object? sender, TappedEventArgs e)
    {
        SetCurrentIndexFromButton(2);
    }

    private async void OnGoToMinusOneClicked(object? sender, TappedEventArgs e)
    {
        await GoToOutOfRangeWithLogAsync(-1);
    }

    private async void OnGoToNinetyNineClicked(object? sender, TappedEventArgs e)
    {
        await GoToOutOfRangeWithLogAsync(99);
    }

    private void OnCurrentIndexChanged(object? sender, int index)
    {
        _currentIndexChangedCount++;
        CurrentIndex = index;
        ResultLabel.Text = $"CurrentIndexChanged #{_currentIndexChangedCount}: {index}";
    }

    private void OnStepActionClicked(object? sender, TappedEventArgs e)
    {
        _stepActionCount++;
        ResultLabel.Text = $"Step action #{_stepActionCount} on CurrentIndex={CurrentIndex}.";
    }

    private void SetCurrentIndexFromButton(int index)
    {
        CurrentIndex = index;
        ResultLabel.Text = $"Requested bound CurrentIndex={index}. Visual state and log expose runtime CurrentIndex binding behavior.";
    }

    private async Task NavigateWithLogAsync(string action, Func<Task> navigate)
    {
        int before = StepsView.CurrentIndex;

        await navigate();

        string status = StepsView.CurrentIndex == before
            ? "unchanged"
            : $"changed to {StepsView.CurrentIndex}";

        ResultLabel.Text = $"{action}: {status}.";
    }

    private async Task GoToOutOfRangeWithLogAsync(int index)
    {
        int before = StepsView.CurrentIndex;

        await StepsView.GoToAsync(index);

        string status = StepsView.CurrentIndex == before
            ? "unchanged"
            : $"changed to {StepsView.CurrentIndex}";

        ResultLabel.Text = $"GoToAsync({index}): {status}.";
    }
}
