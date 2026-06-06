using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class CountdownControlsTests : ContentPage
{
    private int _arcStartedCount;
    private int _arcPausedCount;
    private int _arcStoppedCount;
    private int _arcUpdatedCount;
    private int _arcCompletedCount;

    private int _pieStartedCount;
    private int _piePausedCount;
    private int _pieStoppedCount;
    private int _pieUpdatedCount;
    private int _pieCompletedCount;

    private int _popStartedCount;
    private int _popPausedCount;
    private int _popStoppedCount;
    private int _popCompletedCount;
    private int _popValueChangedCount;
    private int _popDisplayTextChangedCount;

    public CountdownControlsTests()
    {
        InitializeComponent();

        RefreshArcState();
        RefreshPieState();
        RefreshPopState();
    }

    private void OnMainArcStartClicked(object? sender, TappedEventArgs e)
    {
        StartCountdown(MainArcCountdown, "MainArcCountdown");
        RefreshArcState();
    }

    private void OnMainArcPauseClicked(object? sender, TappedEventArgs e)
    {
        MainArcCountdown.Pause();
        RefreshArcState();
        ResultLabel.Text = "MainArcCountdown paused.";
    }

    private void OnMainArcStopClicked(object? sender, TappedEventArgs e)
    {
        MainArcCountdown.Stop();
        RefreshArcState();
        ResultLabel.Text = "MainArcCountdown stopped.";
    }

    private void OnMainPieStartClicked(object? sender, TappedEventArgs e)
    {
        StartCountdown(MainPieCountdown, "MainPieCountdown");
        RefreshPieState();
    }

    private void OnMainPiePauseClicked(object? sender, TappedEventArgs e)
    {
        MainPieCountdown.Pause();
        RefreshPieState();
        ResultLabel.Text = "MainPieCountdown paused.";
    }

    private void OnMainPieStopClicked(object? sender, TappedEventArgs e)
    {
        MainPieCountdown.Stop();
        RefreshPieState();
        ResultLabel.Text = "MainPieCountdown stopped.";
    }

    private async void OnMainPopStartClicked(object? sender, TappedEventArgs e)
    {
        await StartPopCountdown(MainPopCountdown, "MainPopCountdown");
        RefreshPopState();
    }

    private void OnMainPopPauseClicked(object? sender, TappedEventArgs e)
    {
        MainPopCountdown.Pause();
        RefreshPopState();
        ResultLabel.Text = "MainPopCountdown paused.";
    }

    private void OnMainPopStopClicked(object? sender, TappedEventArgs e)
    {
        MainPopCountdown.Stop();
        RefreshPopState();
        ResultLabel.Text = "MainPopCountdown stopped.";
    }

    private void OnMainPopResetClicked(object? sender, TappedEventArgs e)
    {
        MainPopCountdown.Reset();
        RefreshPopState();
        ResultLabel.Text = "MainPopCountdown reset.";
    }

    private async void OnMainPopRestartClicked(object? sender, TappedEventArgs e)
    {
        await MainPopCountdown.RestartAsync();
        RefreshPopState();
        ResultLabel.Text = "MainPopCountdown restarted.";
    }

    private async void OnStartPopVariantsClicked(object? sender, TappedEventArgs e)
    {
        var popViews = FindPopCountdownVariants();

        foreach (var pop in popViews)
        {
            pop.Reset();
            await pop.StartAsync();
        }

        ResultLabel.Text = "Pop variants started.";
    }

    private void OnResetPopVariantsClicked(object? sender, TappedEventArgs e)
    {
        foreach (var pop in FindPopCountdownVariants())
        {
            pop.Reset();
        }

        ResultLabel.Text = "Pop variants reset.";
    }

    private void OnArcCountdownStarted(object? sender, EventArgs e)
    {
        _arcStartedCount++;

        if (sender == MainArcCountdown)
        {
            RefreshArcState();
        }

        ResultLabel.Text = $"Arc Started #{_arcStartedCount}";
    }

    private void OnArcCountdownPaused(object? sender, EventArgs e)
    {
        _arcPausedCount++;

        if (sender == MainArcCountdown)
        {
            RefreshArcState();
        }

        ResultLabel.Text = $"Arc Paused #{_arcPausedCount}";
    }

    private void OnArcCountdownStopped(object? sender, EventArgs e)
    {
        _arcStoppedCount++;

        if (sender == MainArcCountdown)
        {
            RefreshArcState();
        }

        ResultLabel.Text = $"Arc Stopped #{_arcStoppedCount}";
    }

    private void OnArcCountdownUpdated(object? sender, CountdownUpdatedEventArgs e)
    {
        _arcUpdatedCount++;

        if (_arcUpdatedCount % 2 != 0)
        {
            return;
        }

        if (sender == MainArcCountdown)
        {
            RefreshArcState();
        }

        ResultLabel.Text =
            $"Arc Updated #{_arcUpdatedCount}: " +
            $"{e.PreviousValue:0} → {e.CurrentValue:0} | {e.Progress:P0}";
    }

    private void OnArcCountdownCompleted(object? sender, CountdownCompletedEventArgs e)
    {
        _arcCompletedCount++;

        if (sender == MainArcCountdown)
        {
            RefreshArcState();
        }

        ResultLabel.Text = $"Arc Completed #{_arcCompletedCount}: final {e.FinalValue:0}";
    }

    private void OnPieCountdownStarted(object? sender, EventArgs e)
    {
        _pieStartedCount++;

        if (sender == MainPieCountdown)
        {
            RefreshPieState();
        }

        ResultLabel.Text = $"Pie Started #{_pieStartedCount}";
    }

    private void OnPieCountdownPaused(object? sender, EventArgs e)
    {
        _piePausedCount++;

        if (sender == MainPieCountdown)
        {
            RefreshPieState();
        }

        ResultLabel.Text = $"Pie Paused #{_piePausedCount}";
    }

    private void OnPieCountdownStopped(object? sender, EventArgs e)
    {
        _pieStoppedCount++;

        if (sender == MainPieCountdown)
        {
            RefreshPieState();
        }

        ResultLabel.Text = $"Pie Stopped #{_pieStoppedCount}";
    }

    private void OnPieCountdownUpdated(object? sender, CountdownUpdatedEventArgs e)
    {
        _pieUpdatedCount++;

        if (_pieUpdatedCount % 2 != 0)
        {
            return;
        }

        if (sender == MainPieCountdown)
        {
            RefreshPieState();
        }

        ResultLabel.Text =
            $"Pie Updated #{_pieUpdatedCount}: " +
            $"{e.PreviousValue:0} → {e.CurrentValue:0} | {e.Progress:P0}";
    }

    private void OnPieCountdownCompleted(object? sender, CountdownCompletedEventArgs e)
    {
        _pieCompletedCount++;

        if (sender == MainPieCountdown)
        {
            RefreshPieState();
        }

        ResultLabel.Text = $"Pie Completed #{_pieCompletedCount}: final {e.FinalValue:0}";
    }

    private void OnPopStarted(object? sender, EventArgs e)
    {
        _popStartedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text = $"Pop Started #{_popStartedCount}";
    }

    private void OnPopPaused(object? sender, EventArgs e)
    {
        _popPausedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text = $"Pop Paused #{_popPausedCount}";
    }

    private void OnPopStopped(object? sender, EventArgs e)
    {
        _popStoppedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text = $"Pop Stopped #{_popStoppedCount}";
    }

    private void OnPopCompleted(object? sender, EventArgs e)
    {
        _popCompletedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text = $"Pop Completed #{_popCompletedCount}";
    }

    private void OnPopValueChanged(object? sender, PopCountdownValueChangedEventArgs e)
    {
        _popValueChangedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text =
            $"Pop ValueChanged #{_popValueChangedCount}: " +
            $"{e.OldValue} → {e.NewValue}";
    }

    private void OnPopDisplayTextChanged(object? sender, PopCountdownDisplayTextChangedEventArgs e)
    {
        _popDisplayTextChangedCount++;

        if (sender == MainPopCountdown)
        {
            RefreshPopState();
        }

        ResultLabel.Text =
            $"Pop DisplayTextChanged #{_popDisplayTextChangedCount}: " +
            $"{e.OldText} → {e.NewText}";
    }

    private static void StartCountdown(CountdownViewBase countdown, string name)
    {
        try
        {
            countdown.Start();
        }
        catch (Exception ex)
        {
            Application.Current?.MainPage?.DisplayAlert("Start error", $"{name}: {ex.Message}", "OK");
        }
    }

    private async Task StartPopCountdown(PopCountdownView popCountdown, string name)
    {
        try
        {
            await popCountdown.StartAsync();
            ResultLabel.Text = $"{name} started.";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"{name} start error: {ex.Message}";
        }
    }

    private void RefreshArcState()
    {
        ArcStateLabel.Text = $"Arc State: {MainArcCountdown.State}";
        ArcFlagsLabel.Text =
            $"IsRunning={MainArcCountdown.IsRunning} | " +
            $"IsPaused={MainArcCountdown.IsPaused} | " +
            $"IsCompleted={MainArcCountdown.IsCompleted}";
    }

    private void RefreshPieState()
    {
        PieStateLabel.Text = $"Pie State: {MainPieCountdown.State}";
        PieFlagsLabel.Text =
            $"IsRunning={MainPieCountdown.IsRunning} | " +
            $"IsPaused={MainPieCountdown.IsPaused} | " +
            $"IsCompleted={MainPieCountdown.IsCompleted}";
    }

    private void RefreshPopState()
    {
        PopStateLabel.Text = $"Pop IsRunning={MainPopCountdown.IsRunning}";
        PopValueLabel.Text =
            $"Value={MainPopCountdown.Value} | " +
            $"DisplayText={MainPopCountdown.CurrentDisplayText}";
    }

    private IEnumerable<PopCountdownView> FindPopCountdownVariants()
    {
        return FindVisualChildren<PopCountdownView>(this)
            .Where(x => x != MainPopCountdown);
    }

    private static IEnumerable<T> FindVisualChildren<T>(Element root)
        where T : Element
    {
        foreach (var child in root.LogicalChildren)
        {
            if (child is T typed)
            {
                yield return typed;
            }

            foreach (var nested in FindVisualChildren<T>(child))
            {
                yield return nested;
            }
        }
    }
}