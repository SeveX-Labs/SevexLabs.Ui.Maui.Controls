using System.Collections.ObjectModel;
using System.Windows.Input;
using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialSegmentedControlTests : ContentPage
{
    public ObservableCollection<Segment> StatusSegments { get; } = new()
    {
        new Segment("open", "Aperto", "open"),
        new Segment("closed", "Chiuso", "closed"),
        new Segment("pending", "In attesa", "pending")
    };

    public ObservableCollection<Segment> TimeRangeSegments { get; } = new()
    {
        new Segment("today", "Oggi", "today"),
        new Segment("week", "Settimana", "week"),
        new Segment("month", "Mese", "month")
    };

    public ObservableCollection<Segment> CategorySegments { get; } = new()
    {
        new Segment("all", "Tutti", "all"),
        new Segment("appointments", "Appuntamenti", "appointments"),
        new Segment("patients", "Pazienti", "patients"),
        new Segment("reports", "Report", "reports")
    };

    public ObservableCollection<Segment> ShapeSegments { get; } = new()
    {
        new Segment("one", "Primo", 1),
        new Segment("two", "Secondo", 2),
        new Segment("three", "Terzo", 3)
    };

    public ObservableCollection<Segment> PrioritySegments { get; } = new()
    {
        new Segment("low", "Low", 1),
        new Segment("medium", "Medium", 2),
        new Segment("high", "High", 3)
    };

    public ObservableCollection<Segment> GenderSegments { get; } = new()
    {
        new Segment("male", "Maschio", "M"),
        new Segment("female", "Femmina", "F"),
        new Segment("other", "Altro", "O")
    };

    public ObservableCollection<Segment> YesNoSegments { get; } = new()
    {
        new Segment("yes", "Sì", true),
        new Segment("no", "No", false)
    };

    public ObservableCollection<Segment> CommandSegments { get; } = new()
    {
        new Segment("cmd-one", "Uno", 1),
        new Segment("cmd-two", "Due", 2),
        new Segment("cmd-three", "Tre", 3)
    };

    public ObservableCollection<Segment> VisitTypeSegments { get; } = new()
    {
        new Segment("first", "Prima visita", "first"),
        new Segment("check", "Controllo", "check"),
        new Segment("urgent", "Urgente", "urgent")
    };

    private Segment? _selectedStatusSegment;
    private Segment? _selectedTimeRangeSegment;
    private Segment? _selectedCategorySegment;
    private Segment? _selectedShapeSegment;
    private Segment? _selectedPrioritySegment;
    private Segment? _selectedGenderSegment;
    private Segment? _selectedYesNoSegment;
    private Segment? _selectedCommandSegment;
    private Segment? _selectedVisitTypeSegment;

    private int _selectionChangedCount;
    private int _commandCount;

    public MaterialSegmentedControlTests()
    {
        InitializeComponent();

        SegmentSelectedCommand = new Command<Segment?>(OnSegmentSelectedCommandExecuted);

        BindingContext = this;

        SelectedStatusSegment = StatusSegments[0];
        SelectedTimeRangeSegment = TimeRangeSegments[1];
        SelectedCategorySegment = CategorySegments[0];
        SelectedShapeSegment = ShapeSegments[1];
        SelectedPrioritySegment = PrioritySegments[1];
        SelectedGenderSegment = null;
        SelectedYesNoSegment = YesNoSegments[0];
        SelectedCommandSegment = CommandSegments[0];
        SelectedVisitTypeSegment = VisitTypeSegments[0];
    }

    public ICommand SegmentSelectedCommand { get; }

    public Segment? SelectedStatusSegment
    {
        get => _selectedStatusSegment;
        set
        {
            if (_selectedStatusSegment == value)
            {
                return;
            }

            _selectedStatusSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedTimeRangeSegment
    {
        get => _selectedTimeRangeSegment;
        set
        {
            if (_selectedTimeRangeSegment == value)
            {
                return;
            }

            _selectedTimeRangeSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedCategorySegment
    {
        get => _selectedCategorySegment;
        set
        {
            if (_selectedCategorySegment == value)
            {
                return;
            }

            _selectedCategorySegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedShapeSegment
    {
        get => _selectedShapeSegment;
        set
        {
            if (_selectedShapeSegment == value)
            {
                return;
            }

            _selectedShapeSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedPrioritySegment
    {
        get => _selectedPrioritySegment;
        set
        {
            if (_selectedPrioritySegment == value)
            {
                return;
            }

            _selectedPrioritySegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedGenderSegment
    {
        get => _selectedGenderSegment;
        set
        {
            if (_selectedGenderSegment == value)
            {
                return;
            }

            _selectedGenderSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedYesNoSegment
    {
        get => _selectedYesNoSegment;
        set
        {
            if (_selectedYesNoSegment == value)
            {
                return;
            }

            _selectedYesNoSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedCommandSegment
    {
        get => _selectedCommandSegment;
        set
        {
            if (_selectedCommandSegment == value)
            {
                return;
            }

            _selectedCommandSegment = value;
            OnPropertyChanged();
        }
    }

    public Segment? SelectedVisitTypeSegment
    {
        get => _selectedVisitTypeSegment;
        set
        {
            if (_selectedVisitTypeSegment == value)
            {
                return;
            }

            _selectedVisitTypeSegment = value;
            OnPropertyChanged();
        }
    }

    private void OnSegmentSelectionChanged(object? sender, SegmentSelectionChangedEventArgs e)
    {
        _selectionChangedCount++;

        var selectedText = e.SelectedSegment is null
            ? "nessun segmento selezionato"
            : $"{e.SelectedSegment.Label} ({e.SelectedSegment.Id})";

        ResultLabel.Text = $"SelectionChanged #{_selectionChangedCount}: {selectedText}";
    }

    private void OnSegmentSelectedCommandExecuted(Segment? segment)
    {
        _commandCount++;

        var selectedText = segment is null
            ? "nessun segmento selezionato"
            : $"{segment.Label} ({segment.Id})";

        ResultLabel.Text = $"SelectionChangedCommand #{_commandCount}: {selectedText}";
    }

    private void OnSelectLowClicked(object? sender, TappedEventArgs e)
    {
        SelectedPrioritySegment = PrioritySegments.First(x => x.Id == "low");
        ProgrammaticSegmentedControl.SelectSegment(SelectedPrioritySegment);

        ResultLabel.Text = "Selezione programmatica: Low";
    }

    private void OnSelectMediumClicked(object? sender, TappedEventArgs e)
    {
        SelectedPrioritySegment = PrioritySegments.First(x => x.Id == "medium");
        ProgrammaticSegmentedControl.SelectSegment(SelectedPrioritySegment);

        ResultLabel.Text = "Selezione programmatica: Medium";
    }

    private void OnSelectHighClicked(object? sender, TappedEventArgs e)
    {
        SelectedPrioritySegment = PrioritySegments.First(x => x.Id == "high");
        ProgrammaticSegmentedControl.SelectSegment(SelectedPrioritySegment);

        ResultLabel.Text = "Selezione programmatica: High";
    }

    private void OnDeselectPriorityClicked(object? sender, TappedEventArgs e)
    {
        var selected = ProgrammaticSegmentedControl.GetSelectedSegment();

        if (selected is not null)
        {
            ProgrammaticSegmentedControl.DeselectSegment(selected);
            SelectedPrioritySegment = null;
        }

        ResultLabel.Text = "Segmento programmatico deselezionato.";
    }

    private void OnToggleSegmentedErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorSegmentedControl.HasError = !ButtonToggleErrorSegmentedControl.HasError;

        ResultLabel.Text = ButtonToggleErrorSegmentedControl.HasError
            ? "HasError=True applicato al MaterialSegmentedControl tramite bottone."
            : "HasError=False applicato al MaterialSegmentedControl tramite bottone.";
    }

    private void OnResetSegmentedErrorButtonClicked(object? sender, TappedEventArgs e)
    {
        ButtonToggleErrorSegmentedControl.HasError = false;
        SelectedYesNoSegment = YesNoSegments[0];
        ButtonToggleErrorSegmentedControl.SelectSegment(SelectedYesNoSegment);

        ResultLabel.Text = "MaterialSegmentedControl ripristinato tramite bottone.";
    }
}