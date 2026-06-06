using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MediaProgressBarTests : ContentPage
{
    private int _startedCount;
    private int _pausedCount;
    private int _stoppedCount;
    private int _updatedCount;
    private int _completedCount;

    public MediaProgressBarTests()
    {
        InitializeComponent();
    }

    private void OnMainStartClicked(object? sender, TappedEventArgs e)
    {
        StartProgressBar(MainProgressBar, "MainProgressBar");
    }

    private void OnMainPauseClicked(object? sender, TappedEventArgs e)
    {
        MainProgressBar.Pause();
        ResultLabel.Text = "MainProgressBar paused.";
    }

    private void OnMainStopClicked(object? sender, TappedEventArgs e)
    {
        MainProgressBar.Stop();
        ResultLabel.Text = "MainProgressBar stopped.";
    }

    private void OnCardStartClicked(object? sender, TappedEventArgs e)
    {
        StartProgressBar(CardProgressBar, "CardProgressBar");
    }

    private void OnCardPauseClicked(object? sender, TappedEventArgs e)
    {
        CardProgressBar.Pause();
        ResultLabel.Text = "CardProgressBar paused.";
    }

    private void OnCardStopClicked(object? sender, TappedEventArgs e)
    {
        CardProgressBar.Stop();
        ResultLabel.Text = "CardProgressBar stopped.";
    }

    private void OnStartDirectionBarsClicked(object? sender, TappedEventArgs e)
    {
        StartProgressBar(ClockwiseProgressBar, "ClockwiseProgressBar");
        StartProgressBar(ReverseProgressBar, "ReverseProgressBar");
    }

    private void OnStopDirectionBarsClicked(object? sender, TappedEventArgs e)
    {
        ClockwiseProgressBar.Stop();
        ReverseProgressBar.Stop();

        ResultLabel.Text = "Direction progress bars stopped.";
    }

    private void OnCountdownStarted(object? sender, EventArgs e)
    {
        _startedCount++;

        ResultLabel.Text = $"CountdownStarted #{_startedCount}: {GetProgressName(sender)}";
    }

    private void OnCountdownPaused(object? sender, EventArgs e)
    {
        _pausedCount++;

        ResultLabel.Text = $"CountdownPaused #{_pausedCount}: {GetProgressName(sender)}";
    }

    private void OnCountdownStopped(object? sender, EventArgs e)
    {
        _stoppedCount++;

        ResultLabel.Text = $"CountdownStopped #{_stoppedCount}: {GetProgressName(sender)}";
    }

    private void OnCountdownUpdated(object? sender, CountdownUpdatedEventArgs e)
    {
        _updatedCount++;

        if (_updatedCount % 3 != 0)
        {
            return;
        }

        ResultLabel.Text =
            $"CountdownUpdated #{_updatedCount}: {GetProgressName(sender)} | " +
            $"{e.PreviousValue:0} → {e.CurrentValue:0} | progress {e.Progress:P0}";
    }

    private void OnCountdownCompleted(object? sender, CountdownCompletedEventArgs e)
    {
        _completedCount++;

        ResultLabel.Text = $"CountdownCompleted #{_completedCount}: {GetProgressName(sender)} | final {e.FinalValue:0}";
    }

    private void StartProgressBar(MediaProgressBar progressBar, string name)
    {
        try
        {
            progressBar.Start();
            ResultLabel.Text = $"{name} started.";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"{name} start error: {ex.Message}";
        }
    }

    private static string GetProgressName(object? sender)
    {
        return sender switch
        {
            MediaProgressBar progressBar when !string.IsNullOrWhiteSpace(progressBar.AutomationId) => progressBar.AutomationId,
            MediaProgressBar => "MediaProgressBar",
            _ => "Unknown"
        };
    }
}