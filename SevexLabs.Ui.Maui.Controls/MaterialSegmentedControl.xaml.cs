using Microsoft.Maui.Layouts;
using SevexLabs.Ui.Maui.Controls.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SevexLabs.Ui.Maui.Controls;

public partial class MaterialSegmentedControl
{
    #region fields

    private bool _fromUserInteraction;

    private Color? _normalBorderColor;
    private double? _normalBorderThickness;
    private bool _isApplyingErrorVisualState;

    #endregion

    #region bindable properties

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MaterialSegmentedControl),
            default(IEnumerable),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SegmentsWidthModeProperty =
        BindableProperty.Create(
            nameof(SegmentsWidthMode),
            typeof(SegmentWidthMode),
            typeof(MaterialSegmentedControl),
            defaultValue: SegmentWidthMode.Equal,
            propertyChanged: OnWidthModePropertyChanged);

    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(
            nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(MaterialSegmentedControl),
            default(ICommand));

    public static readonly BindableProperty SelectedSegmentProperty =
        BindableProperty.Create(
            nameof(SelectedSegment),
            typeof(Segment),
            typeof(MaterialSegmentedControl),
            default(Segment),
            BindingMode.TwoWay,
            propertyChanged: OnSelectedSegmentChanged);

    public static readonly BindableProperty AllowEmptySelectionProperty =
        BindableProperty.Create(
            nameof(AllowEmptySelection),
            typeof(bool),
            typeof(MaterialSegmentedControl),
            true,
            propertyChanged: (bindable, _, _) => ((MaterialSegmentedControl)bindable).ApplyAllowEmptySelection());

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(MaterialSegmentedControl),
            defaultValue: false,
            propertyChanged: (bindable, _, _) => ((MaterialSegmentedControl)bindable).ApplyHasError());

    public static readonly BindableProperty ErrorColorProperty =
        BindableProperty.Create(
            nameof(ErrorColor),
            typeof(Color),
            typeof(MaterialSegmentedControl),
            defaultValue: Color.FromArgb("#FF0000"),
            propertyChanged: (bindable, _, _) => ((MaterialSegmentedControl)bindable).ApplyBorderColor());

    public static readonly BindableProperty ErrorBorderThicknessProperty =
        BindableProperty.Create(
            nameof(ErrorBorderThickness),
            typeof(double),
            typeof(MaterialSegmentedControl),
            2d,
            propertyChanged: (bindable, _, _) => ((MaterialSegmentedControl)bindable).ApplyBorderThickness());

    public static readonly BindableProperty SegmentStyleProperty =
        BindableProperty.Create(
            nameof(SegmentStyle),
            typeof(Style),
            typeof(MaterialSegmentedControl),
            defaultValueCreator: bindable =>
            {
                if ((Application.Current?.Resources.TryGetValue("DefaultSegment", out var styleObj) ?? false) &&
                    styleObj is Style style)
                {
                    return style;
                }

                return null;
            });

    #endregion

    #region properties

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public SegmentWidthMode SegmentsWidthMode
    {
        get => (SegmentWidthMode)GetValue(SegmentsWidthModeProperty);
        set => SetValue(SegmentsWidthModeProperty, value);
    }

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public Segment? SelectedSegment
    {
        get => (Segment?)GetValue(SelectedSegmentProperty);
        set => SetValue(SelectedSegmentProperty, value);
    }

    public bool AllowEmptySelection
    {
        get => (bool)GetValue(AllowEmptySelectionProperty);
        set => SetValue(AllowEmptySelectionProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public Color ErrorColor
    {
        get => (Color)GetValue(ErrorColorProperty);
        set => SetValue(ErrorColorProperty, value);
    }

    public double ErrorBorderThickness
    {
        get => (double)GetValue(ErrorBorderThicknessProperty);
        set => SetValue(ErrorBorderThicknessProperty, value);
    }

    public Style? SegmentStyle
    {
        get => (Style?)GetValue(SegmentStyleProperty);
        set => SetValue(SegmentStyleProperty, value);
    }

    #endregion

    #region auto-properties

    public ObservableCollection<Segment> Items { get; } = new();

    private Action<Segment?>? ActOnSegmentSelectionChanged { get; set; }

    #endregion

    #region events

    public event EventHandler<SegmentSelectionChangedEventArgs>? SegmentSelectionChanged;

    #endregion

    #region ctor(s)

    public MaterialSegmentedControl()
    {
        InitializeComponent();
    }

    #endregion

    #region access methods

    public void Configure(Action<Segment> actOnSegmentSelectionChanged)
    {
        ActOnSegmentSelectionChanged = actOnSegmentSelectionChanged;
    }

    public void Initialize(IList<Segment>? segments)
    {
        Dispatcher.AssertOnUi<MaterialSegmentedControl>();

        Items.Clear();

        if (segments is null)
        {
            return;
        }

        foreach (var segment in segments)
        {
            Items.Add(segment);
        }

        EnsureSelectionIfRequired();
        UpdateSelectedItemsBinding();
        RefreshSegmentsVisualState();
    }

    public Segment? GetSelectedSegment()
    {
        return Items.FirstOrDefault(i => i.IsSelected);
    }

    public T? GetSelectedSegmentData<T>()
    {
        return Items
            .Where(i => i.Tag is not null && i.Tag is T && i.IsSelected)
            .Select(i => i.Tag)
            .Cast<T>()
            .FirstOrDefault();
    }

    public void SelectSegment(Segment segment)
    {
        Dispatcher.AssertOnUi<MaterialSegmentedControl>();

        foreach (var item in Items)
        {
            item.WithIsSelected(item.Id == segment.Id);
        }

        EnsureSelectionIfRequired();
        UpdateSelectedItemsBinding();
        RefreshSegmentsVisualState();
    }

    public void DeselectSegment(Segment segment)
    {
        Dispatcher.AssertOnUi<MaterialSegmentedControl>();

        if (!AllowEmptySelection &&
            IsSelectedSegment(segment))
        {
            return;
        }

        foreach (var item in Items)
        {
            if (!string.IsNullOrEmpty(item.Id) &&
                !string.IsNullOrEmpty(segment.Id) &&
                item.Id == segment.Id)
            {
                item.WithIsSelected(false);
            }
        }

        EnsureSelectionIfRequired();
        UpdateSelectedItemsBinding();
        RefreshSegmentsVisualState();
    }

    #endregion

    #region overrides

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (_isApplyingErrorVisualState)
        {
            return;
        }

        if (propertyName == BorderColorProperty.PropertyName)
        {
            if (!HasError)
            {
                _normalBorderColor = null;
            }

            return;
        }

        if (propertyName == BorderThicknessProperty.PropertyName)
        {
            if (!HasError)
            {
                _normalBorderThickness = null;
            }
        }
    }

    #endregion

    #region event handlers

    private void HandleSegmentTappedInternal(object sender, EventArgs e)
    {
        if (e is not TappedEventArgs { Parameter: Segment segment })
        {
            return;
        }

        var target = !segment.IsSelected;

        if (!target && !AllowEmptySelection)
        {
            return;
        }

        _fromUserInteraction = true;

        try
        {
            foreach (var item in Items)
            {
                item.WithIsSelected(item.Id == segment.Id && target);
            }

            EnsureSelectionIfRequired();
            UpdateSelectedItemsBinding();
            RefreshSegmentsVisualState();
        }
        finally
        {
            if (SelectedSegment == null)
            {
                _fromUserInteraction = false;
            }
        }
    }

    private static void OnWidthModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (MaterialSegmentedControl)bindable;

        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            var widthMode = (SegmentWidthMode)newValue;

            switch (widthMode)
            {
                case SegmentWidthMode.Equal:
                    self.HorizontalOptions = LayoutOptions.Fill;
                    self.FlexContainer.AlignItems = FlexAlignItems.Stretch;
                    break;

                case SegmentWidthMode.Auto:
                    self.HorizontalOptions = LayoutOptions.Start;
                    self.FlexContainer.AlignItems = FlexAlignItems.Start;
                    break;
            }
        });
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (MaterialSegmentedControl)bindable;

        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (newValue is null)
            {
                self.Initialize(null);
                return;
            }

            if (newValue is IList<Segment> list)
            {
                self.Initialize(list);
                return;
            }

            if (newValue is IEnumerable enumerable)
            {
                var listFromEnum = enumerable
                    .Cast<object>()
                    .OfType<Segment>()
                    .ToList();

                self.Initialize(listFromEnum);
            }
        });
    }

    private static void OnSelectedSegmentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MaterialSegmentedControl)bindable;

        if (control._fromUserInteraction)
        {
            control.RaiseSelectionChanged();
            control._fromUserInteraction = false;
            return;
        }

        _ = MainThread.InvokeOnMainThreadAsync(() =>
            control.ApplySelectedSegmentFromBinding(newValue as Segment));
    }
    
    private void RaiseSelectionChanged()
    {
        var selectedSegment = GetSelectedSegment();

        ActOnSegmentSelectionChanged?.Invoke(selectedSegment);
        SegmentSelectionChanged?.Invoke(this, new SegmentSelectionChangedEventArgs(selectedSegment));

        if (SelectionChangedCommand?.CanExecute(selectedSegment) == true)
        {
            SelectionChangedCommand.Execute(selectedSegment);
        }
    }

    #endregion

    #region error visual state

    private void ApplyHasError()
    {
        ApplyBorderColor();
        ApplyBorderThickness();

        RefreshSegmentsVisualState();
    }

    private void RefreshSegmentsVisualState()
    {
        Dispatcher.Dispatch(() =>
        {
            foreach (var child in FlexContainer.Children)
            {
                if (child is MaterialSegment segment)
                {
                    segment.ApplyVisualState();
                }
            }

            FlexContainer.InvalidateMeasure();
            InvalidateMeasure();
        });
    }

    private void ApplyBorderColor()
    {
        if (HasError)
        {
            _normalBorderColor ??= BorderColor;
            SetBorderColorWithoutTracking(ErrorColor);
            return;
        }

        if (_normalBorderColor is not null)
        {
            SetBorderColorWithoutTracking(_normalBorderColor);
            _normalBorderColor = null;
        }
    }

    private void ApplyBorderThickness()
    {
        if (HasError)
        {
            _normalBorderThickness ??= BorderThickness;
            SetBorderThicknessWithoutTracking(ErrorBorderThickness);
            return;
        }

        if (_normalBorderThickness.HasValue)
        {
            SetBorderThicknessWithoutTracking(_normalBorderThickness.Value);
            _normalBorderThickness = null;
        }
    }

    private void SetBorderColorWithoutTracking(Color value)
    {
        _isApplyingErrorVisualState = true;

        try
        {
            BorderColor = value;
        }
        finally
        {
            _isApplyingErrorVisualState = false;
        }
    }

    private void SetBorderThicknessWithoutTracking(double value)
    {
        _isApplyingErrorVisualState = true;

        try
        {
            BorderThickness = value;
        }
        finally
        {
            _isApplyingErrorVisualState = false;
        }
    }

    #endregion

    #region helper methods

    private void UpdateSelectedItemsBinding()
    {
        var selected = GetSelectedSegment();
        SelectedSegment = selected;
    }

    private void ApplySelectedSegmentFromBinding(Segment? selected)
    {
        if (!Items.Any())
        {
            return;
        }

        if (selected is null)
        {
            if (!AllowEmptySelection)
            {
                EnsureSelectionIfRequired();
                UpdateSelectedItemsBinding();
                RefreshSegmentsVisualState();
                RaiseSelectionChanged();
                return;
            }

            foreach (var segment in Items)
            {
                segment.WithIsSelected(false);
            }

            RefreshSegmentsVisualState();
            RaiseSelectionChanged();
            return;
        }

        foreach (var item in Items)
        {
            item.WithIsSelected(item.Id == selected.Id);
        }

        EnsureSelectionIfRequired();
        UpdateSelectedItemsBinding();
        RefreshSegmentsVisualState();
        RaiseSelectionChanged();
    }

    private void ApplyAllowEmptySelection()
    {
        if (AllowEmptySelection)
        {
            return;
        }

        EnsureSelectionIfRequired();
        UpdateSelectedItemsBinding();
        RefreshSegmentsVisualState();
    }

    private void EnsureSelectionIfRequired()
    {
        if (AllowEmptySelection)
        {
            return;
        }

        if (!Items.Any())
        {
            return;
        }

        if (GetSelectedSegment() is not null)
        {
            return;
        }

        var firstSegment = Items.FirstOrDefault();

        if (firstSegment is null)
        {
            return;
        }

        firstSegment.WithIsSelected(true);
    }

    private bool IsSelectedSegment(Segment segment)
    {
        var selectedSegment = GetSelectedSegment();

        if (selectedSegment is null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(selectedSegment.Id) ||
            string.IsNullOrEmpty(segment.Id))
        {
            return ReferenceEquals(selectedSegment, segment);
        }

        return selectedSegment.Id == segment.Id;
    }

    #endregion
}