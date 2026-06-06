using System.Collections.ObjectModel;
using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class ControlsGalleryPage : ContentPage
{
    public ObservableCollection<string> VisitTypes { get; } = new()
    {
        "First visit",
        "Follow-up",
        "Remote consultation"
    };

    public ObservableCollection<string> WorkflowChips { get; } = new()
    {
        "Forms",
        "Reports",
        "Billing",
        "Calendar"
    };

    public IList<object> SelectedWorkflowChips { get; } = new List<object>
    {
        "Forms",
        "Reports"
    };

    public ObservableCollection<Segment> DashboardSegments { get; } = new()
    {
        new Segment("today", "Today", "today"),
        new Segment("week", "Week", "week"),
        new Segment("month", "Month", "month")
    };

    public Segment? SelectedDashboardSegment { get; set; }

    public ControlsGalleryPage()
    {
        InitializeComponent();

        SelectedDashboardSegment = DashboardSegments[1];
        BindingContext = this;
        VisitTypePicker.SelectedIndex = 1;
    }

    private void OnFastBorderClicked(object? sender, TappedEventArgs e)
    {
        ShowSection(FastBorderSection);
    }

    private void OnInputsClicked(object? sender, TappedEventArgs e)
    {
        ShowSection(InputsSection);
    }

    private void OnSelectionClicked(object? sender, TappedEventArgs e)
    {
        ShowSection(SelectionSection);
    }

    private void OnTextStepsClicked(object? sender, TappedEventArgs e)
    {
        ShowSection(TextStepsSection);
    }

    private void OnProgressClicked(object? sender, TappedEventArgs e)
    {
        ShowSection(ProgressSection);
    }

    private void ShowSection(View selectedSection)
    {
        FastBorderSection.IsVisible = ReferenceEquals(selectedSection, FastBorderSection);
        InputsSection.IsVisible = ReferenceEquals(selectedSection, InputsSection);
        SelectionSection.IsVisible = ReferenceEquals(selectedSection, SelectionSection);
        TextStepsSection.IsVisible = ReferenceEquals(selectedSection, TextStepsSection);
        ProgressSection.IsVisible = ReferenceEquals(selectedSection, ProgressSection);
    }
}
