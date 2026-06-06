using PropertyChanged;
using SevexLabs.Ui.Maui.Controls.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace SevexLabs.Ui.Maui.Controls;

public partial class MaterialFlexChips
{
    #region nested classes

    [AddINotifyPropertyChangedInterface]
    public class SelectableChip
    {
        #region auto-properties

        public int Index { get; }

        public bool IsSelected { get; set; }

        [AlsoNotifyFor(nameof(Label), nameof(Id))]
        public object? Tag { get; set; }

        [AlsoNotifyFor(nameof(NormalizedSpacing))]
        public Thickness Spacing { get; internal set; }

        [AlsoNotifyFor(nameof(NormalizedSpacing))]
        public bool IsFirst { get; internal set; }

        [AlsoNotifyFor(nameof(NormalizedSpacing))]
        public bool IsLast { get; internal set; }

        #endregion

        #region properties

        public string Id => Tag is string ? (string)Tag : Tag is IChip chipDefinition ? chipDefinition.Id : string.Empty;

        public string Label =>
            Tag is string ? (string)Tag : Tag is IChip chipDefinition ? chipDefinition.Label : string.Empty;

        public Thickness NormalizedSpacing => IsFirst && IsLast ? new Thickness(0, Spacing.Top, 0, Spacing.Bottom)
            : IsFirst ? new Thickness(0, Spacing.Top, Spacing.Right, Spacing.Bottom)
            : IsLast ? new Thickness(Spacing.Left, Spacing.Top, 0, Spacing.Bottom)
            : Spacing;

        #endregion

        #region ctor(s)

        public SelectableChip(int index, object? tag, Thickness spacing)
        {
            Index = index;
            Tag = tag;
            Spacing = spacing;
        }

        #endregion

        #region access methods

        public void WithSpacing(Thickness spacing)
        {
            Spacing = spacing;
        }

        #endregion
    }

    #endregion

    #region bindable properties

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MaterialFlexChips),
            default(IEnumerable),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode),
        typeof(SelectionMode),
        typeof(MaterialFlexChips),
        defaultValue: SelectionMode.Single,
        propertyChanged: (b, ov, nv) =>
        {
            var self = (MaterialFlexChips)b;
            switch ((SelectionMode)nv)
            {
                case SelectionMode.Single:
                    {
                        var first = self.Items.FirstOrDefault(i => i.IsSelected);
                        foreach (var it in self.Items)
                            it.IsSelected = ReferenceEquals(it, first);
                        self.UpdateSelectedItemsBinding();
                        break;
                    }

                case SelectionMode.None:
                    {
                        foreach (var it in self.Items)
                            it.IsSelected = false;
                        self.UpdateSelectedItemsBinding();
                        break;
                    }

                    // Multiple: nessun reset
            }
        });

    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing),
        typeof(Thickness),
        typeof(MaterialFlexChips),
        defaultValue: new Thickness(0, 0, 10, 7),
        propertyChanged: (bindable, _, _) =>
        {
            if (bindable is MaterialFlexChips materialFlexChips)
                materialFlexChips.OnStylePropertyChanged();
        });

    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(
            nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(MaterialFlexChips),
            default(ICommand));

    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(
            nameof(SelectedItems),
            typeof(IList<object>),
            typeof(MaterialFlexChips),
            default(IList<object>),
            BindingMode.TwoWay,
            propertyChanged: OnSelectedItemsChanged);

    public static readonly BindableProperty ChipStyleProperty =
        BindableProperty.Create(
            nameof(ChipStyle),
            typeof(Style),
            typeof(MaterialFlexChips),
            defaultValueCreator: bindable =>
            {
                if ((Application.Current?.Resources.TryGetValue("DefaultFlexChip", out var styleObj) ?? false) && styleObj is Style style)
                    return style;

                return null;
            });

    public static readonly BindableProperty ChipTemplateProperty =
        BindableProperty.Create(
            nameof(ChipTemplate),
            typeof(DataTemplate),
            typeof(MaterialFlexChips),
            default(DataTemplate),
            propertyChanged: OnChipTemplateChanged);

    #endregion

    #region fields

    private bool _fromUserInteraction;

    private DataTemplate? _defaultChipTemplate;

    private DataTemplate? _effectiveChipTemplate;

    #endregion

    #region properties

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public Thickness Spacing
    {
        get => (Thickness)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public IList<object>? SelectedItems
    {
        get => (IList<object>?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public Style? ChipStyle
    {
        get => (Style?)GetValue(ChipStyleProperty);
        set => SetValue(ChipStyleProperty, value);
    }

    public DataTemplate? ChipTemplate
    {
        get => (DataTemplate?)GetValue(ChipTemplateProperty);
        set => SetValue(ChipTemplateProperty, value);
    }

    public DataTemplate? EffectiveChipTemplate
    {
        get => _effectiveChipTemplate;
        private set
        {
            if (ReferenceEquals(_effectiveChipTemplate, value))
                return;

            _effectiveChipTemplate = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region auto-properties

    public ObservableCollection<SelectableChip> Items { get; set; } = new();

    private Action<IEnumerable<object>>? ActOnSelectionChanged { get; set; }

    #endregion

    #region events

    public event EventHandler<ChipSelectionChangedEventArgs>? SelectionChanged;

    #endregion

    #region ctor(s)

    public MaterialFlexChips()
    {
        InitializeComponent();

        _defaultChipTemplate = Resources.TryGetValue("DefaultChipTemplate", out var defaultTemplate)
            ? defaultTemplate as DataTemplate
            : null;

        ApplyEffectiveChipTemplate();

        HookItemsCollectionChanged();
        MarkFirstLast();
    }

    #endregion

    #region access methods

    public void Configure(Action<IEnumerable<object>> actOnSelectionChanged)
    {
        ActOnSelectionChanged = actOnSelectionChanged;
    }

    public void Initialize(IEnumerable? enumerable)
    {
        if (enumerable is null)
        {
            ReplaceItemsCollection(new ObservableCollection<SelectableChip>());
            return;
        }

        var selectableChips = new List<SelectableChip>();
        int index = 0;

        foreach (var current in enumerable)
        {
            selectableChips.Add(new SelectableChip(index, current, Spacing));
            index++;
        }

        if (selectableChips.Count > 0)
        {
            selectableChips[0].IsFirst = true;
            selectableChips[^1].IsLast = true;
        }

        ReplaceItemsCollection(new ObservableCollection<SelectableChip>(selectableChips));
    }

    public IEnumerable<object> GetSelectedItems()
    {
        return Items
            .Where(i => i.Tag is not null && i.IsSelected)
            .Select(i => i.Tag!)
            .ToList();
    }

    public IEnumerable<T> GetSelectedItems<T>()
    {
        return Items.Where(i => !(i.Tag is null) && i.Tag is T && i.IsSelected)
            .Select(i => i.Tag)
            .Cast<T>().ToList();
    }

    public void SelectItem(string id)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();

        if (SelectionMode == SelectionMode.None) return;

        foreach (var item in Items)
        {
            if (SelectionMode == SelectionMode.Single)
                item.IsSelected = item.Id == id;
            else // Multiple → additivo
                if (item.Id == id) item.IsSelected = true;
        }

        UpdateSelectedItemsBinding();
    }

    public void DeselectItem(string id)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();

        foreach (var item in Items)
        {
            if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(id) && item.Id == id)
                item.IsSelected = false;
        }

        UpdateSelectedItemsBinding();
    }

    public void SelectItems(IEnumerable<string>? itemsToSelect)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();
        if (SelectionMode == SelectionMode.None) return;

        var list = itemsToSelect?.ToList() ?? new List<string>();
        if (SelectionMode == SelectionMode.Single && list.Count() > 1)
        {
            SelectItem(list.First());
        }
        else
        {
            foreach (var item in Items)
                item.IsSelected = !string.IsNullOrEmpty(item.Id)
                                  && list.Contains(item.Id);

            UpdateSelectedItemsBinding();
        }
    }

    public void SelectItem(IChip itemToSelect)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();
        if (SelectionMode == SelectionMode.None) return;

        foreach (var item in Items)
        {
            if (SelectionMode == SelectionMode.Single)
                item.IsSelected = !string.IsNullOrEmpty(item.Id)
                                  // && ((item is IChip chipDefinition && chipDefinition.Id == itemToSelect.Id) || (item is SelectableChip selectableChip && selectableChip.Id == itemToSelect.Id))
                                  && item.Id == itemToSelect.Id;
            else // Multiple → additivo
                if (item.Id == itemToSelect.Id) item.IsSelected = true;
        }

        UpdateSelectedItemsBinding();
    }

    public void DeselectItem(IChip itemToSelect)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();

        foreach (var item in Items)
        {
            if (!string.IsNullOrEmpty(item.Id)
                // && ((item is IChip chipDefinition && chipDefinition.Id == itemToSelect.Id) || (item is SelectableChip selectableChip && selectableChip.Id == itemToSelect.Id)))
                && item.Id == itemToSelect.Id)
            {
                item.IsSelected = false;
            }
        }

        UpdateSelectedItemsBinding();
    }

    public void SelectItems(IEnumerable<IChip>? itemsToSelect)
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();
        if (SelectionMode == SelectionMode.None) return;

        var list = itemsToSelect?.ToList() ?? new List<IChip>();
        if (SelectionMode == SelectionMode.Single && list.Count() > 1)
        {
            SelectItem(list.First());
        }
        else
        {
            foreach (var item in Items)
            {
                item.IsSelected = !string.IsNullOrEmpty(item.Id)
                                  && !(itemsToSelect is null)
                                  && list.Any(i => i.Id == item.Id);
                // && ((item is IChip chipDefinition && iEnumerable.Any(i => i.Id == chipDefinition.Id)) || (item is SelectableChip selectableChip && iEnumerable.Any(i => i.Id == selectableChip.Id)))
            }

            UpdateSelectedItemsBinding();
        }
    }

    public void DeselectAll()
    {
        Dispatcher.AssertOnUi<MaterialFlexChips>();
        foreach (var item in Items)
            item.IsSelected = false;

        UpdateSelectedItemsBinding();
    }

    #endregion

    #region event handlers

    private void HandleChipTappedInternal(object sender, EventArgs e)
    {
        if (SelectionMode == SelectionMode.None) return;
        if (e is not TappedEventArgs { Parameter: SelectableChip chip }) return;

        _fromUserInteraction = true;

        var target = !chip.IsSelected;
        if (SelectionMode == SelectionMode.Single)
        {
            foreach (var item in Items)
                item.IsSelected = (item.Index == chip.Index) && target;
        }
        else if (SelectionMode == SelectionMode.Multiple)
        {
            chip.IsSelected = target;
        }

        UpdateSelectedItemsBinding();
    }

    private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var oldList = oldValue as IList<object>;
        var newList = newValue as IList<object>;

        var control = (MaterialFlexChips)bindable;
        if (control._fromUserInteraction)
        {
            control.RaiseSelectionChanged();
            control._fromUserInteraction = false;
            return;
        }

        if (oldList.SequenceEqualShallow(newList)) return;
        _ = MainThread.InvokeOnMainThreadAsync(() => control.ApplySelectedItemsFromBinding(newList));
    }

    internal void OnStylePropertyChanged()
    {
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var item in Items)
                item.WithSpacing(Spacing);
        });
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (MaterialFlexChips)bindable;
        _ = MainThread.InvokeOnMainThreadAsync(() => self.Initialize(newValue as IEnumerable));
    }

    private static void OnChipTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (MaterialFlexChips)bindable;
        self.ApplyEffectiveChipTemplate();
    }

    #endregion

    #region helper methods

    private void ApplyEffectiveChipTemplate()
    {
        EffectiveChipTemplate = ChipTemplate ?? _defaultChipTemplate;
    }

    private void MarkFirstLast()
    {
        if (!Items.Any())
            return;

        for (int i = 0; i < Items.Count; i++)
        {
            var chip = Items[i];
            chip.IsFirst = i == 0;
            chip.IsLast = i == Items.Count - 1;
        }
    }

    private void HookItemsCollectionChanged()
    {
        if (Items is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= OnItemsCollectionChanged;
            incc.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void ReplaceItemsCollection(ObservableCollection<SelectableChip> newItems)
    {
        if (Items is INotifyCollectionChanged oldIncc)
            oldIncc.CollectionChanged -= OnItemsCollectionChanged;

        Items = newItems;

        HookItemsCollectionChanged();
        MarkFirstLast();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MarkFirstLast();
    }

    private void UpdateSelectedItemsBinding()
    {
        var selected = GetSelectedItems().ToList();

        if (SelectedItems is { } current && current.SequenceEqualShallow(selected))
        {
            _fromUserInteraction = false;
            return;
        }

        SelectedItems = selected;
    }

    private void ApplySelectedItemsFromBinding(IList<object>? selected)
    {
        if (!Items.Any())
            return;

        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            // if (SelectionMode == SelectionMode.None || selected is null || !selected.Any())
            if (selected is null || !selected.Any())
            {
                foreach (var chip in Items)
                    chip.IsSelected = false;

                // SelectedItems = Array.Empty<object>();
                RaiseSelectionChanged();
                return;
            }

            var selectedSet = new HashSet<object>(selected);
            if (selectedSet.All(x => x is string))
            {
                var stringSet = selectedSet.Cast<string>().ToList();
                if (SelectionMode == SelectionMode.Single && stringSet.Count > 1)
                    stringSet = new List<string> { stringSet[0] };

                foreach (var item in Items)
                    item.IsSelected = !string.IsNullOrEmpty(item.Id) && stringSet.Contains(item.Id);

                RaiseSelectionChanged();
            }
            else if (selectedSet.All(x => x is IChip))
            {
                var chipSet = selectedSet.Cast<IChip>().Select(c => c.Id).ToHashSet();

                if (SelectionMode == SelectionMode.Single && chipSet.Count > 1)
                    chipSet = new HashSet<string> { chipSet.First() };

                foreach (var item in Items)
                    item.IsSelected = !string.IsNullOrEmpty(item.Id) && chipSet.Contains(item.Id);

                RaiseSelectionChanged();
            }
        });
    }

    private void RaiseSelectionChanged()
    {
        var selectedItems = GetSelectedItems().ToList();

        ActOnSelectionChanged?.Invoke(selectedItems);
        SelectionChanged?.Invoke(this, new ChipSelectionChangedEventArgs(selectedItems));

        SelectionChangedCommand?.Execute(selectedItems);
    }

    #endregion
}
