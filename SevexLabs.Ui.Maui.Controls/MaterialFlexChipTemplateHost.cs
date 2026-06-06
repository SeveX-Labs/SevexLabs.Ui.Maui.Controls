namespace SevexLabs.Ui.Maui.Controls;

internal class MaterialFlexChipTemplateHost : ContentView
{
    #region bindable properties

    public static readonly BindableProperty ChipProperty =
        BindableProperty.Create(
            nameof(Chip),
            typeof(MaterialFlexChips.SelectableChip),
            typeof(MaterialFlexChipTemplateHost),
            default(MaterialFlexChips.SelectableChip),
            propertyChanged: OnTemplateHostPropertyChanged);

    public static readonly BindableProperty ChipTemplateProperty =
        BindableProperty.Create(
            nameof(ChipTemplate),
            typeof(DataTemplate),
            typeof(MaterialFlexChipTemplateHost),
            default(DataTemplate),
            propertyChanged: OnTemplateHostPropertyChanged);

    #endregion

    #region properties

    public MaterialFlexChips.SelectableChip? Chip
    {
        get => (MaterialFlexChips.SelectableChip?)GetValue(ChipProperty);
        set => SetValue(ChipProperty, value);
    }

    public DataTemplate? ChipTemplate
    {
        get => (DataTemplate?)GetValue(ChipTemplateProperty);
        set => SetValue(ChipTemplateProperty, value);
    }

    #endregion

    #region event handlers

    private static void OnTemplateHostPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MaterialFlexChipTemplateHost host)
        {
            host.BuildContent();
        }
    }

    #endregion

    #region helper methods

    private void BuildContent()
    {
        if (Chip is null || ChipTemplate is null)
        {
            Content = null;
            return;
        }

        var content = ChipTemplate.CreateContent();

        if (content is View view)
        {
            view.BindingContext = Chip;
            Content = view;
            return;
        }

        if (content is ViewCell viewCell)
        {
            viewCell.View.BindingContext = Chip;
            Content = viewCell.View;
            return;
        }

        Content = null;
    }

    #endregion
}