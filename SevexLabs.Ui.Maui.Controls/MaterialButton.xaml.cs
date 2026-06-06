using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// Button-like material control with optional icon and loading indicator.
/// </summary>
/// <remarks>
/// Loading changes content visibility only; effective padding still comes from the applied style,
/// XAML values, or dynamic resources.
/// </remarks>
public partial class MaterialButton
{
    public enum IconPositions
    {
        Left = 0,
        Right = 1
    }

    // -----------------------
    // Events
    // -----------------------
    /// <summary>
    /// Raised when the control receives a button-like tap and is not loading or disabled.
    /// </summary>
    public event EventHandler<TappedEventArgs>? Clicked;


    // -----------------------
    // Bindable properties
    // -----------------------

    public static readonly BindableProperty ClickCommandProperty =
        BindableProperty.Create(
            nameof(ClickCommand),
            typeof(ICommand),
            typeof(MaterialButton),
            defaultValue: null,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is not MaterialButton b) return;

                if (oldVal is ICommand oldCmd)
                    oldCmd.CanExecuteChanged -= b.OnCommandCanExecuteChanged;

                if (newVal is ICommand newCmd)
                    newCmd.CanExecuteChanged += b.OnCommandCanExecuteChanged;

                b.UpdateCommandCanExecute();
            });

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(MaterialButton),
            defaultValue: null,
            propertyChanged: (bindable, _, __) =>
            {
                if (bindable is MaterialButton b)
                    b.UpdateCommandCanExecute();
            });

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(MaterialButton),
            defaultValue: string.Empty,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b)
                {
                    b.TextView.Text = (string)newVal;
                    b.UpdateContentVisibilityAndLayout();
                }
            }));

    public static readonly BindableProperty TextTransformProperty =
        BindableProperty.Create(
            nameof(TextTransform),
            typeof(TextTransform),
            typeof(MaterialButton),
            defaultValue: Microsoft.Maui.TextTransform.Default,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is Microsoft.Maui.TextTransform tt)
                    b.TextView.TextTransform = tt;
            }));

    public static readonly BindableProperty UseIconProperty =
        BindableProperty.Create(
            nameof(UseIcon),
            typeof(bool),
            typeof(MaterialButton),
            defaultValue: false,
            propertyChanged: (bindable, _, __) =>
            {
                if (bindable is MaterialButton b)
                    b.UpdateContentVisibilityAndLayout();
            });

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(MaterialButton),
            defaultValue: null,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b)
                {
                    b.IconView.Source = (ImageSource?)newVal;
                    b.UpdateContentVisibilityAndLayout();
                }
            }));

    public static readonly BindableProperty IconPositionProperty =
        BindableProperty.Create(
            nameof(IconPosition),
            typeof(IconPositions),
            typeof(MaterialButton),
            defaultValue: IconPositions.Left,
            propertyChanged: (bindable, _, __) =>
            {
                if (bindable is MaterialButton b)
                    b.UpdateIconPosition();
            });

    public static readonly BindableProperty IconSizeProperty =
        BindableProperty.Create(
            nameof(IconSize),
            typeof(double),
            typeof(MaterialButton),
            defaultValue: 18d,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is double s)
                {
                    b.IconView.HeightRequest = s;
                    b.IconView.WidthRequest = s; // icona quadrata
                }
            }));

    public static readonly BindableProperty IconSpacingProperty =
        BindableProperty.Create(
            nameof(IconSpacing),
            typeof(double),
            typeof(MaterialButton),
            defaultValue: 0d,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is double sp)
                    b.ContentGrid.ColumnSpacing = sp;
            }));

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MaterialButton),
            defaultValue: Colors.White,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is Color c)
                    b.TextView.TextColor = c;
            }));

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(MaterialButton),
            defaultValue: null,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b)
                    b.TextView.FontFamily = (string?)newVal;
            }));

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MaterialButton),
            defaultValue: 16d,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is double s)
                    b.TextView.FontSize = s;
            }));

    public static readonly BindableProperty LoadingColorProperty =
        BindableProperty.Create(
            nameof(LoadingColor),
            typeof(Color),
            typeof(MaterialButton),
            defaultValue: Colors.White,
            propertyChanged: (BindableProperty.BindingPropertyChangedDelegate)((bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is Color c)
                    b.LoadingView.Color = c;
            }));

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(MaterialButton),
            defaultValue: false,
            propertyChanged: (bindable, _, newVal) =>
            {
                if (bindable is MaterialButton b && newVal is bool isLoading)
                {
                    b.ApplyLoading(isLoading);
                    b.UpdateCommandCanExecute();
                }
            });

    // La mantengo: piccola, non impatta, ed evita breaking change.
    // Inoltre l'effetto "pill -> spinner compatto" � spesso voluto.
    public static readonly BindableProperty MustReduceToLoadingProperty =
        BindableProperty.Create(
            nameof(MustReduceToLoading),
            typeof(bool),
            typeof(MaterialButton),
            defaultValue: false,
            propertyChanged: (bindable, _, __) =>
            {
                if (bindable is MaterialButton b)
                    b.ApplyLoading(b.IsLoading);
            });

    // -----------------------
    // CLR wrappers
    // -----------------------
    /// <summary>
    /// Gets or sets the command executed by a tap.
    /// </summary>
    /// <remarks>
    /// The command is executed only when <see cref="ICommand.CanExecute"/> allows it,
    /// and command availability is mirrored to <see cref="VisualElement.IsEnabled"/>.
    /// </remarks>
    public ICommand? ClickCommand
    {
        get => (ICommand?)GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter passed to <see cref="ClickCommand"/> and to the tap event args.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextTransform TextTransform
    {
        get => (TextTransform)GetValue(TextTransformProperty);
        set => SetValue(TextTransformProperty, value);
    }

    public bool UseIcon
    {
        get => (bool)GetValue(UseIconProperty);
        set => SetValue(UseIconProperty, value);
    }

    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public IconPositions IconPosition
    {
        get => (IconPositions)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public double IconSpacing
    {
        get => (double)GetValue(IconSpacingProperty);
        set => SetValue(IconSpacingProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color LoadingColor
    {
        get => (Color)GetValue(LoadingColorProperty);
        set => SetValue(LoadingColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the loading indicator is shown.
    /// </summary>
    /// <remarks>
    /// While loading, taps are ignored and command availability is recalculated.
    /// </remarks>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether loading collapses the normal content to a compact spinner-only state.
    /// </summary>
    /// <remarks>
    /// When false, loading keeps the normal content footprint while hiding it visually.
    /// </remarks>
    public bool MustReduceToLoading
    {
        get => (bool)GetValue(MustReduceToLoadingProperty);
        set => SetValue(MustReduceToLoadingProperty, value);
    }

    // -----------------------
    // Ctor
    // -----------------------
    public MaterialButton()
    {
        InitializeComponent();

        // inizializza layout coerente con i default
        IconView.HeightRequest = IconSize;
        IconView.WidthRequest = IconSize;
        ContentGrid.ColumnSpacing = IconSpacing;
        UpdateIconPosition();
        UpdateContentVisibilityAndLayout();
        ApplyLoading(IsLoading);
        UpdateCommandCanExecute();
    }

    // -----------------------
    // Tap
    // -----------------------
    private void OnTapped(object? sender, EventArgs e)
    {
        if (IsLoading) return;
        if (!IsEnabled) return;

        var param = CommandParameter;

        var cmd = ClickCommand;
        if (cmd is not null && cmd.CanExecute(param))
            cmd.Execute(param);

        // Passo il parametro anche all'evento Clicked (EventArgs derivato, non rompe chi usa EventArgs)
        Clicked?.Invoke(this, new TappedEventArgs(param));
    }

    // -----------------------
    // Helpers
    // -----------------------
    private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
        => UpdateCommandCanExecute();

    private void UpdateCommandCanExecute()
    {
        var cmd = ClickCommand;
        var param = CommandParameter;

        bool can = cmd?.CanExecute(param) ?? true;
        IsEnabled = can;
    }

    private void UpdateContentVisibilityAndLayout()
    {
        var hasIcon = UseIcon && IconSource is not null;
        var hasText = !string.IsNullOrEmpty(Text);

        IconView.IsVisible = hasIcon;
        TextView.IsVisible = hasText;

        // se ho solo icona o solo testo, lo spacing deve essere 0
        ContentGrid.ColumnSpacing = (hasIcon && hasText) ? IconSpacing : 0;

        // se non ho testo, metto l'icona in colonna 0 e svuoto la colonna 1 (label nascosta)
        // la centratura la gestisce ContentGrid con HorizontalOptions="Center".
        UpdateIconPosition();
    }

    private void UpdateIconPosition()
    {
        // Layout a 2 colonne: [0]=icona, [1]=testo (di default).
        // Se icon a destra: scambio le colonne.
        if (IconPosition == IconPositions.Left)
        {
            Grid.SetColumn(IconView, 0);
            Grid.SetColumn(TextView, 1);
        }
        else
        {
            Grid.SetColumn(TextView, 0);
            Grid.SetColumn(IconView, 1);
        }
    }

    private void ApplyLoading(bool isLoading)
    {
        if (ContentGrid.Height != -1 && LoadingView.WidthRequest < ContentGrid.Height)
            LoadingView.WidthRequest = LoadingView.HeightRequest = ContentGrid.Height;

        LoadingView.IsVisible = LoadingView.IsRunning = isLoading;

        InputTransparent = isLoading;

        if (!isLoading)
        {
            ContentGrid.IsVisible = true;
            ContentGrid.Opacity = 1;
            ContentGrid.WidthRequest = -1;
            ContentGrid.HeightRequest = -1;

            LoadingView.IsVisible = LoadingView.IsRunning = false;
        }
        else if (!MustReduceToLoading)
        {
            ContentGrid.IsVisible = true;
            ContentGrid.Opacity = 0;
            ContentGrid.WidthRequest = -1;
            ContentGrid.HeightRequest = -1;
        }
        else
        {
            ContentGrid.IsVisible = false;
            ContentGrid.Opacity = 0;
            ContentGrid.WidthRequest = 0;
            ContentGrid.HeightRequest = 0;
        }

        InvalidateMeasure();
    }
}
