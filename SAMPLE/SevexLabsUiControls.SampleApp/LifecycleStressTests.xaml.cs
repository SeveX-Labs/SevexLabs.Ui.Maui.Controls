using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class LifecycleStressTests : ContentPage
{
    private readonly ArcCountdown _arcCountdown;
    private readonly MediaProgressBar _mediaProgressBar;
    private readonly PopCountdownView _popCountdownView;
    private readonly ShimmerView _shimmerView;
    private readonly ShimmerLayout _shimmerLayout;
    private readonly VerticalStackLayout _shimmerContent;

    private int _arcMounts;
    private int _arcUpdates;
    private int _arcUpdatesAfterUnmount;
    private int _arcCompleted;

    private int _mediaMounts;
    private int _mediaUpdates;
    private int _mediaUpdatesAfterUnmount;
    private int _mediaCompleted;

    private int _popMounts;
    private int _popValueChanged;
    private int _popDisplayChanged;
    private int _popUpdatesAfterUnmount;
    private int _popCompleted;

    private int _shimmerMounts;

    public LifecycleStressTests()
    {
        InitializeComponent();

        _arcCountdown = CreateArcCountdown();
        _mediaProgressBar = CreateMediaProgressBar();
        _popCountdownView = CreatePopCountdownView();
        _shimmerView = CreateShimmerView();
        _shimmerLayout = CreateShimmerLayout();
        _shimmerContent = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                _shimmerView,
                _shimmerLayout
            }
        };

        MountArc("Arc mounted.");
        MountMedia("MediaProgressBar mounted.");
        MountPop("PopCountdownView mounted.");
        MountShimmer("Shimmer controls mounted.");
    }

    private ArcCountdown CreateArcCountdown()
    {
        var countdown = new ArcCountdown
        {
            WidthRequest = 180,
            HeightRequest = 180,
            HorizontalOptions = LayoutOptions.Center,
            StartValue = 30,
            FinalValue = 0,
            StepValue = 1,
            StepInterval = TimeSpan.FromMilliseconds(250),
            Animate = true,
            AnimationDuration = 120,
            MaxDecimals = 0,
            ValueSuffix = "s",
            FontSize = 28,
            TextColor = Colors.Black,
            TrackColor = Color.FromArgb("#E5E7EB"),
            ProgressColor = Color.FromArgb("#2563EB"),
            TrackThickness = 16,
            StrokeCap = ArcStrokeCap.Round
        };

        countdown.CountdownUpdated += OnArcCountdownUpdated;
        countdown.CountdownCompleted += OnArcCountdownCompleted;

        return countdown;
    }

    private MediaProgressBar CreateMediaProgressBar()
    {
        var progressBar = new MediaProgressBar
        {
            StartValue = 30,
            FinalValue = 0,
            StepValue = 1,
            StepInterval = TimeSpan.FromMilliseconds(250),
            Animate = true,
            AnimationDuration = 120,
            FontSize = 14,
            TextColor = Colors.Black,
            ProgressColor = Color.FromArgb("#16A34A"),
            TrackColor = Color.FromArgb("#DCFCE7"),
            ProgressBarHeight = 12,
            ContainerBackgroundColor = Colors.Transparent,
            ContainerPadding = new Thickness(0, 18, 0, 0)
        };

        progressBar.CountdownUpdated += OnMediaProgressBarUpdated;
        progressBar.CountdownCompleted += OnMediaProgressBarCompleted;

        return progressBar;
    }

    private PopCountdownView CreatePopCountdownView()
    {
        var pop = new PopCountdownView
        {
            HeightRequest = 200,
            HorizontalOptions = LayoutOptions.Fill,
            StartValue = 8,
            EndValue = 0,
            Interval = TimeSpan.FromMilliseconds(350),
            AnimationDuration = TimeSpan.FromMilliseconds(160),
            BackgroundColor = Color.FromArgb("#111827"),
            TextColor = Colors.White,
            DepthColor = Color.FromArgb("#4B5563"),
            FontSize = 74,
            StartScale = 0.1d,
            EndScale = 1.35d,
            FinalTexts = new List<string> { "GO" }
        };

        pop.ValueChanged += OnPopValueChanged;
        pop.DisplayTextChanged += OnPopDisplayTextChanged;
        pop.Completed += OnPopCompleted;

        return pop;
    }

    private static ShimmerView CreateShimmerView()
    {
        return new ShimmerView
        {
            HeightRequest = 44,
            HorizontalOptions = LayoutOptions.Fill,
            CornerRadius = 10,
            Duration = 800,
            IsLoading = true,
            BackgroundGradientColor = Color.FromArgb("#E5E7EB"),
            ShimmerGradientColor = Colors.White
        };
    }

    private static ShimmerLayout CreateShimmerLayout()
    {
        return new ShimmerLayout
        {
            HeightRequest = 92,
            HorizontalOptions = LayoutOptions.Fill,
            Duration = 1000,
            IsLoading = true,
            BackgroundGradientColor = Color.FromArgb("#E5E7EB"),
            ShimmerGradientColor = Colors.White,
            Child = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(0),
                Children =
                {
                    new BoxView
                    {
                        HeightRequest = 24,
                        CornerRadius = 8,
                        HorizontalOptions = LayoutOptions.Fill,
                        Color = Color.FromArgb("#CBD5E1")
                    },
                    new BoxView
                    {
                        HeightRequest = 18,
                        WidthRequest = 220,
                        CornerRadius = 8,
                        HorizontalOptions = LayoutOptions.Start,
                        Color = Color.FromArgb("#CBD5E1")
                    },
                    new BoxView
                    {
                        HeightRequest = 18,
                        WidthRequest = 150,
                        CornerRadius = 8,
                        HorizontalOptions = LayoutOptions.Start,
                        Color = Color.FromArgb("#CBD5E1")
                    }
                }
            }
        };
    }

    private bool IsArcMounted => ArcHost.Content == _arcCountdown;

    private bool IsMediaMounted => MediaHost.Content == _mediaProgressBar;

    private bool IsPopMounted => PopHost.Content == _popCountdownView;

    private bool IsShimmerMounted => ShimmerHost.Content == _shimmerContent;

    private void OnArcMountClicked(object? sender, TappedEventArgs e) => MountArc("Arc mounted.");

    private void OnArcUnmountClicked(object? sender, TappedEventArgs e) => UnmountArc("Arc unmounted while preserving the same instance.");

    private void OnArcRemountClicked(object? sender, TappedEventArgs e)
    {
        UnmountArc();
        MountArc("Arc remounted with the same instance.");
    }

    private void OnArcCycleClicked(object? sender, TappedEventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            UnmountArc();
            MountArc();
        }

        UpdateArcStatus("Arc cycled 10 times.");
    }

    private void OnArcStartClicked(object? sender, TappedEventArgs e)
    {
        TryRun(() => _arcCountdown.Start(), UpdateArcStatus, "Arc started.");
    }

    private void OnArcStopClicked(object? sender, TappedEventArgs e)
    {
        _arcCountdown.Stop();
        UpdateArcStatus("Arc stopped through public Stop().");
    }

    private void OnArcResetClicked(object? sender, TappedEventArgs e)
    {
        _arcUpdates = 0;
        _arcUpdatesAfterUnmount = 0;
        _arcCompleted = 0;
        UpdateArcStatus("Arc counters reset.");
    }

    private void MountArc(string? status = null)
    {
        if (!IsArcMounted)
        {
            ArcHost.Content = _arcCountdown;
            _arcMounts++;
        }

        UpdateArcStatus(status ?? "Arc mounted.");
    }

    private void UnmountArc(string? status = null)
    {
        if (IsArcMounted)
        {
            ArcHost.Content = null;
        }

        if (status is not null)
        {
            UpdateArcStatus(status);
        }
    }

    private void OnArcCountdownUpdated(object? sender, CountdownUpdatedEventArgs e)
    {
        _arcUpdates++;
        if (!IsArcMounted)
        {
            _arcUpdatesAfterUnmount++;
        }

        UpdateArcStatus($"Arc update #{_arcUpdates}: {e.CurrentValue:0}");
    }

    private void OnArcCountdownCompleted(object? sender, CountdownCompletedEventArgs e)
    {
        _arcCompleted++;
        UpdateArcStatus("Arc completed.");
    }

    private void UpdateArcStatus(string status)
    {
        ArcCountersLabel.Text = $"Mounts: {_arcMounts} | Updates: {_arcUpdates} | After unmount: {_arcUpdatesAfterUnmount} | Completed: {_arcCompleted}";
        ArcStatusLabel.Text = $"{status} State={_arcCountdown.State}, Value={_arcCountdown.CurrentValue:0}";
    }

    private void OnMediaMountClicked(object? sender, TappedEventArgs e) => MountMedia("MediaProgressBar mounted.");

    private void OnMediaUnmountClicked(object? sender, TappedEventArgs e) => UnmountMedia("MediaProgressBar unmounted while preserving the same instance.");

    private void OnMediaRemountClicked(object? sender, TappedEventArgs e)
    {
        UnmountMedia();
        MountMedia("MediaProgressBar remounted with the same instance.");
    }

    private void OnMediaCycleClicked(object? sender, TappedEventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            UnmountMedia();
            MountMedia();
        }

        UpdateMediaStatus("MediaProgressBar cycled 10 times.");
    }

    private void OnMediaStartClicked(object? sender, TappedEventArgs e)
    {
        TryRun(() => _mediaProgressBar.Start(), UpdateMediaStatus, "MediaProgressBar started.");
    }

    private void OnMediaStopClicked(object? sender, TappedEventArgs e)
    {
        _mediaProgressBar.Stop();
        UpdateMediaStatus("MediaProgressBar stopped through public Stop().");
    }

    private void OnMediaResetClicked(object? sender, TappedEventArgs e)
    {
        _mediaUpdates = 0;
        _mediaUpdatesAfterUnmount = 0;
        _mediaCompleted = 0;
        UpdateMediaStatus("MediaProgressBar counters reset.");
    }

    private void MountMedia(string? status = null)
    {
        if (!IsMediaMounted)
        {
            MediaHost.Content = _mediaProgressBar;
            _mediaMounts++;
        }

        UpdateMediaStatus(status ?? "MediaProgressBar mounted.");
    }

    private void UnmountMedia(string? status = null)
    {
        if (IsMediaMounted)
        {
            MediaHost.Content = null;
        }

        if (status is not null)
        {
            UpdateMediaStatus(status);
        }
    }

    private void OnMediaProgressBarUpdated(object? sender, CountdownUpdatedEventArgs e)
    {
        _mediaUpdates++;
        if (!IsMediaMounted)
        {
            _mediaUpdatesAfterUnmount++;
        }

        UpdateMediaStatus($"MediaProgressBar update #{_mediaUpdates}: {e.CurrentValue:0}");
    }

    private void OnMediaProgressBarCompleted(object? sender, CountdownCompletedEventArgs e)
    {
        _mediaCompleted++;
        UpdateMediaStatus("MediaProgressBar completed.");
    }

    private void UpdateMediaStatus(string status)
    {
        MediaCountersLabel.Text = $"Mounts: {_mediaMounts} | Updates: {_mediaUpdates} | After unmount: {_mediaUpdatesAfterUnmount} | Completed: {_mediaCompleted}";
        MediaStatusLabel.Text = $"{status} State={_mediaProgressBar.State}, Value={_mediaProgressBar.CurrentValue:0}";
    }

    private void OnPopMountClicked(object? sender, TappedEventArgs e) => MountPop("PopCountdownView mounted.");

    private void OnPopUnmountClicked(object? sender, TappedEventArgs e) => UnmountPop("PopCountdownView unmounted while preserving the same instance.");

    private void OnPopRemountClicked(object? sender, TappedEventArgs e)
    {
        UnmountPop();
        MountPop("PopCountdownView remounted with the same instance.");
    }

    private void OnPopCycleClicked(object? sender, TappedEventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            UnmountPop();
            MountPop();
        }

        UpdatePopStatus("PopCountdownView cycled 10 times.");
    }

    private async void OnPopStartClicked(object? sender, TappedEventArgs e)
    {
        try
        {
            await _popCountdownView.StartAsync();
            UpdatePopStatus("PopCountdownView started.");
        }
        catch (Exception ex)
        {
            UpdatePopStatus($"Start failed: {ex.Message}");
        }
    }

    private void OnPopStopClicked(object? sender, TappedEventArgs e)
    {
        _popCountdownView.Stop();
        UpdatePopStatus("PopCountdownView stopped through public Stop().");
    }

    private void OnPopResetClicked(object? sender, TappedEventArgs e)
    {
        _popValueChanged = 0;
        _popDisplayChanged = 0;
        _popUpdatesAfterUnmount = 0;
        _popCompleted = 0;
        UpdatePopStatus("PopCountdownView counters reset.");
    }

    private void MountPop(string? status = null)
    {
        if (!IsPopMounted)
        {
            PopHost.Content = _popCountdownView;
            _popMounts++;
        }

        UpdatePopStatus(status ?? "PopCountdownView mounted.");
    }

    private void UnmountPop(string? status = null)
    {
        if (IsPopMounted)
        {
            PopHost.Content = null;
        }

        if (status is not null)
        {
            UpdatePopStatus(status);
        }
    }

    private void OnPopValueChanged(object? sender, PopCountdownValueChangedEventArgs e)
    {
        _popValueChanged++;
        CountPopAfterUnmount();
        UpdatePopStatus($"Pop ValueChanged #{_popValueChanged}: {e.NewValue}");
    }

    private void OnPopDisplayTextChanged(object? sender, PopCountdownDisplayTextChangedEventArgs e)
    {
        _popDisplayChanged++;
        CountPopAfterUnmount();
        UpdatePopStatus($"Pop DisplayTextChanged #{_popDisplayChanged}: {e.NewText}");
    }

    private void OnPopCompleted(object? sender, EventArgs e)
    {
        _popCompleted++;
        CountPopAfterUnmount();
        UpdatePopStatus("PopCountdownView completed.");
    }

    private void CountPopAfterUnmount()
    {
        if (!IsPopMounted)
        {
            _popUpdatesAfterUnmount++;
        }
    }

    private void UpdatePopStatus(string status)
    {
        PopCountersLabel.Text = $"Mounts: {_popMounts} | ValueChanged: {_popValueChanged} | DisplayChanged: {_popDisplayChanged} | After unmount: {_popUpdatesAfterUnmount} | Completed: {_popCompleted}";
        PopStatusLabel.Text = $"{status} IsRunning={_popCountdownView.IsRunning}, Value={_popCountdownView.Value}, Text={_popCountdownView.CurrentDisplayText}";
    }

    private void OnShimmerMountClicked(object? sender, TappedEventArgs e) => MountShimmer("Shimmer controls mounted.");

    private void OnShimmerUnmountClicked(object? sender, TappedEventArgs e) => UnmountShimmer("Shimmer controls unmounted while preserving the same instances.");

    private void OnShimmerRemountClicked(object? sender, TappedEventArgs e)
    {
        UnmountShimmer();
        MountShimmer("Shimmer controls remounted with the same instances.");
    }

    private void OnShimmerCycleClicked(object? sender, TappedEventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            UnmountShimmer();
            MountShimmer();
        }

        UpdateShimmerStatus("Shimmer controls cycled 10 times.");
    }

    private void OnShimmerLoadingOnClicked(object? sender, TappedEventArgs e)
    {
        SetShimmerLoading(true);
        UpdateShimmerStatus("Shimmer loading enabled.");
    }

    private void OnShimmerLoadingOffClicked(object? sender, TappedEventArgs e)
    {
        SetShimmerLoading(false);
        UpdateShimmerStatus("Shimmer loading disabled.");
    }

    private void OnShimmerResetClicked(object? sender, TappedEventArgs e)
    {
        _shimmerMounts = 0;
        UpdateShimmerStatus("Shimmer counters reset.");
    }

    private void MountShimmer(string? status = null)
    {
        if (!IsShimmerMounted)
        {
            ShimmerHost.Content = _shimmerContent;
            _shimmerMounts++;
        }

        UpdateShimmerStatus(status ?? "Shimmer controls mounted.");
    }

    private void UnmountShimmer(string? status = null)
    {
        if (IsShimmerMounted)
        {
            ShimmerHost.Content = null;
        }

        if (status is not null)
        {
            UpdateShimmerStatus(status);
        }
    }

    private void SetShimmerLoading(bool isLoading)
    {
        _shimmerView.IsLoading = isLoading;
        _shimmerLayout.IsLoading = isLoading;
    }

    private void UpdateShimmerStatus(string status)
    {
        ShimmerCountersLabel.Text = $"Mounts: {_shimmerMounts} | IsLoading: {_shimmerView.IsLoading}";
        ShimmerStatusLabel.Text = status;
    }

    private static void TryRun(Action action, Action<string> updateStatus, string successStatus)
    {
        try
        {
            action();
            updateStatus(successStatus);
        }
        catch (Exception ex)
        {
            updateStatus($"Action failed: {ex.Message}");
        }
    }
}
