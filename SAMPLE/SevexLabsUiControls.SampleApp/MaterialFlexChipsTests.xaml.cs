using System.Collections.ObjectModel;
using System.Windows.Input;
using SevexLabs.Ui.Maui.Controls;

namespace SevexLabsUiControls.SampleApp;

public partial class MaterialFlexChipsTests : ContentPage
{
    public ObservableCollection<TestChip> StatusChips { get; } = new()
    {
        new TestChip("open", "Aperto"),
        new TestChip("closed", "Chiuso"),
        new TestChip("pending", "In attesa")
    };

    public ObservableCollection<TestChip> TimeRangeChips { get; } = new()
    {
        new TestChip("today", "Oggi"),
        new TestChip("week", "Settimana"),
        new TestChip("month", "Mese"),
        new TestChip("year", "Anno")
    };

    public ObservableCollection<TestChip> CategoryChips { get; } = new()
    {
        new TestChip("appointments", "Appuntamenti"),
        new TestChip("patients", "Pazienti"),
        new TestChip("reports", "Report"),
        new TestChip("invoices", "Fatture"),
        new TestChip("anamnesis", "Anamnesi"),
        new TestChip("therapy", "Terapia")
    };

    public ObservableCollection<TestChip> DisabledChips { get; } = new()
    {
        new TestChip("one", "Non selezionabile"),
        new TestChip("two", "Solo display"),
        new TestChip("three", "SelectionMode None")
    };

    public ObservableCollection<TestChip> ShapeChips { get; } = new()
    {
        new TestChip("one", "Primo"),
        new TestChip("two", "Secondo"),
        new TestChip("three", "Terzo"),
        new TestChip("four", "Quarto")
    };

    public ObservableCollection<TestChip> CompactChips { get; } = new()
    {
        new TestChip("a", "A"),
        new TestChip("b", "B"),
        new TestChip("c", "C"),
        new TestChip("d", "D"),
        new TestChip("e", "E"),
        new TestChip("f", "F")
    };

    public ObservableCollection<TestChip> PriorityChips { get; } = new()
    {
        new TestChip("low", "Low"),
        new TestChip("medium", "Medium"),
        new TestChip("high", "High")
    };

    public ObservableCollection<TestChip> SkillChips { get; } = new()
    {
        new TestChip("csharp", "C#"),
        new TestChip("maui", ".NET MAUI"),
        new TestChip("xamarin", "Xamarin"),
        new TestChip("android", "Android"),
        new TestChip("ios", "iOS"),
        new TestChip("mongodb", "MongoDB"),
        new TestChip("s3", "Amazon S3")
    };

    public ObservableCollection<TestChip> CommandChips { get; } = new()
    {
        new TestChip("cmd-one", "Uno"),
        new TestChip("cmd-two", "Due"),
        new TestChip("cmd-three", "Tre"),
        new TestChip("cmd-four", "Quattro")
    };

    public ObservableCollection<string> StringChips { get; } = new()
    {
        "String A",
        "String B",
        "String C",
        "String D"
    };

    public ObservableCollection<TestChip> VisitTypeChips { get; } = new()
    {
        new TestChip("first", "Prima visita"),
        new TestChip("check", "Controllo"),
        new TestChip("urgent", "Urgente")
    };

    private IList<object>? _selectedStatusChips;
    private IList<object>? _selectedTimeRangeChips;
    private IList<object>? _selectedCategoryChips;
    private IList<object>? _selectedDisabledChips;
    private IList<object>? _selectedShapeChips;
    private IList<object>? _selectedCompactChips;
    private IList<object>? _selectedLargeSpacingChips;
    private IList<object>? _selectedPriorityChips;
    private IList<object>? _selectedSkillChips;
    private IList<object>? _selectedCommandChips;
    private IList<object>? _selectedStringChips;
    private IList<object>? _selectedCategoryCardChips;
    private IList<object>? _selectedVisitTypeChips;

    private int _selectionChangedCount;
    private int _commandCount;

    public MaterialFlexChipsTests()
    {
        InitializeComponent();

        ChipsSelectedCommand = new Command<IEnumerable<object>>(OnChipsSelectedCommandExecuted);

        BindingContext = this;

        SelectedStatusChips = new List<object> { StatusChips[0] };
        SelectedTimeRangeChips = new List<object> { TimeRangeChips[1] };
        SelectedCategoryChips = new List<object> { CategoryChips[0], CategoryChips[2] };
        SelectedDisabledChips = new List<object>();
        SelectedShapeChips = new List<object> { ShapeChips[1] };
        SelectedCompactChips = new List<object> { CompactChips[0], CompactChips[2] };
        SelectedLargeSpacingChips = new List<object> { CompactChips[1], CompactChips[3] };
        SelectedPriorityChips = new List<object> { PriorityChips[1] };
        SelectedSkillChips = new List<object> { SkillChips[0], SkillChips[1] };
        SelectedCommandChips = new List<object> { CommandChips[0] };
        SelectedStringChips = new List<object> { "String B", "String D" };
        SelectedCategoryCardChips = new List<object> { CategoryChips[1], CategoryChips[4] };
        SelectedVisitTypeChips = new List<object> { VisitTypeChips[0] };
    }

    public ICommand ChipsSelectedCommand { get; }

    public IList<object>? SelectedStatusChips
    {
        get => _selectedStatusChips;
        set
        {
            if (ReferenceEquals(_selectedStatusChips, value))
            {
                return;
            }

            _selectedStatusChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedTimeRangeChips
    {
        get => _selectedTimeRangeChips;
        set
        {
            if (ReferenceEquals(_selectedTimeRangeChips, value))
            {
                return;
            }

            _selectedTimeRangeChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedCategoryChips
    {
        get => _selectedCategoryChips;
        set
        {
            if (ReferenceEquals(_selectedCategoryChips, value))
            {
                return;
            }

            _selectedCategoryChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedDisabledChips
    {
        get => _selectedDisabledChips;
        set
        {
            if (ReferenceEquals(_selectedDisabledChips, value))
            {
                return;
            }

            _selectedDisabledChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedShapeChips
    {
        get => _selectedShapeChips;
        set
        {
            if (ReferenceEquals(_selectedShapeChips, value))
            {
                return;
            }

            _selectedShapeChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedCompactChips
    {
        get => _selectedCompactChips;
        set
        {
            if (ReferenceEquals(_selectedCompactChips, value))
            {
                return;
            }

            _selectedCompactChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedLargeSpacingChips
    {
        get => _selectedLargeSpacingChips;
        set
        {
            if (ReferenceEquals(_selectedLargeSpacingChips, value))
            {
                return;
            }

            _selectedLargeSpacingChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedPriorityChips
    {
        get => _selectedPriorityChips;
        set
        {
            if (ReferenceEquals(_selectedPriorityChips, value))
            {
                return;
            }

            _selectedPriorityChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedSkillChips
    {
        get => _selectedSkillChips;
        set
        {
            if (ReferenceEquals(_selectedSkillChips, value))
            {
                return;
            }

            _selectedSkillChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedCommandChips
    {
        get => _selectedCommandChips;
        set
        {
            if (ReferenceEquals(_selectedCommandChips, value))
            {
                return;
            }

            _selectedCommandChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedStringChips
    {
        get => _selectedStringChips;
        set
        {
            if (ReferenceEquals(_selectedStringChips, value))
            {
                return;
            }

            _selectedStringChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedCategoryCardChips
    {
        get => _selectedCategoryCardChips;
        set
        {
            if (ReferenceEquals(_selectedCategoryCardChips, value))
            {
                return;
            }

            _selectedCategoryCardChips = value;
            OnPropertyChanged();
        }
    }

    public IList<object>? SelectedVisitTypeChips
    {
        get => _selectedVisitTypeChips;
        set
        {
            if (ReferenceEquals(_selectedVisitTypeChips, value))
            {
                return;
            }

            _selectedVisitTypeChips = value;
            OnPropertyChanged();
        }
    }

    private void OnChipSelectionChanged(object? sender, ChipSelectionChangedEventArgs e)
    {
        _selectionChangedCount++;

        ResultLabel.Text = $"SelectionChanged #{_selectionChangedCount}: {FormatSelectedItems(e.SelectedItems)}";
    }

    private void OnChipsSelectedCommandExecuted(IEnumerable<object> selectedItems)
    {
        _commandCount++;

        ResultLabel.Text = $"SelectionChangedCommand #{_commandCount}: {FormatSelectedItems(selectedItems)}";
    }

    private void OnSelectLowClicked(object? sender, TappedEventArgs e)
    {
        SelectedPriorityChips = new List<object> { PriorityChips[0] };
        ProgrammaticFlexChips.SelectItem("low");

        ResultLabel.Text = "Selezione programmatica: Low";
    }

    private void OnSelectMediumClicked(object? sender, TappedEventArgs e)
    {
        SelectedPriorityChips = new List<object> { PriorityChips[1] };
        ProgrammaticFlexChips.SelectItem("medium");

        ResultLabel.Text = "Selezione programmatica: Medium";
    }

    private void OnSelectHighClicked(object? sender, TappedEventArgs e)
    {
        SelectedPriorityChips = new List<object> { PriorityChips[2] };
        ProgrammaticFlexChips.SelectItem("high");

        ResultLabel.Text = "Selezione programmatica: High";
    }

    private void OnDeselectPriorityClicked(object? sender, TappedEventArgs e)
    {
        SelectedPriorityChips = new List<object>();
        ProgrammaticFlexChips.DeselectAll();

        ResultLabel.Text = "Priority chips deselezionati.";
    }

    private void OnSelectCSharpMauiClicked(object? sender, TappedEventArgs e)
    {
        SelectedSkillChips = new List<object>
        {
            SkillChips.First(x => x.Id == "csharp"),
            SkillChips.First(x => x.Id == "maui")
        };

        ProgrammaticMultipleFlexChips.SelectItems(new[] { "csharp", "maui" });

        ResultLabel.Text = "Selezione multipla programmatica: C# + .NET MAUI";
    }

    private void OnClearSkillsClicked(object? sender, TappedEventArgs e)
    {
        SelectedSkillChips = new List<object>();
        ProgrammaticMultipleFlexChips.DeselectAll();

        ResultLabel.Text = "Skill chips deselezionati.";
    }

    private static string FormatSelectedItems(IEnumerable<object>? selectedItems)
    {
        if (selectedItems is null)
        {
            return "nessuna selezione";
        }

        var labels = selectedItems
            .Select(GetLabel)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return labels.Count == 0
            ? "nessuna selezione"
            : string.Join(", ", labels);
    }

    private static string GetLabel(object item)
    {
        return item switch
        {
            IChip chip => chip.Label,
            string text => text,
            _ => item.ToString() ?? string.Empty
        };
    }

    public sealed class TestChip : IChip
    {
        public TestChip(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }

        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
    }
}