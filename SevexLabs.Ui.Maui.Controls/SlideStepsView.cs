using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// XAML-friendly slide steps view that accepts child <see cref="View"/> elements directly,
/// preserving their order as written.
/// Provides next/previous navigation with a slide animation.
/// </summary>
[ContentProperty(nameof(Slides))]
public class SlideStepsView : Grid
{
    private bool _isAnimating;
    private bool _initialized;

    public SlideStepsView()
    {
        IsClippedToBounds = true; // keep slides within the bounds during animation
        Slides = new ObservableCollection<View>();
        Slides.CollectionChanged += OnSlidesCollectionChanged;
    }

    #region Slides (XAML content)

    /// <summary>
    /// Collection of slide views. In XAML you can declare child elements directly inside
    /// a <see cref="SlideStepsView"/>.
    /// </summary>
    public ObservableCollection<View> Slides { get; }

    #endregion

    #region CurrentIndex bindable property

    public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(
        nameof(CurrentIndex), typeof(int), typeof(SlideStepsView), 0,
        propertyChanged: async (b, o, n) => await ((SlideStepsView)b).CoercedGoToAsync((int)n));

    /// <summary>
    /// Zero-based index of the currently visible slide.
    /// </summary>
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    #endregion

    #region Animation configuration

    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration), typeof(uint), typeof(SlideStepsView), (uint)300);

    /// <summary>
    /// Duration in milliseconds for the slide animation.
    /// </summary>
    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly BindableProperty AnimationEasingProperty = BindableProperty.Create(
        nameof(AnimationEasing), typeof(Easing), typeof(SlideStepsView), Easing.CubicOut);

    /// <summary>
    /// Easing used by the slide animation.
    /// </summary>
    public Easing AnimationEasing
    {
        get => (Easing)GetValue(AnimationEasingProperty);
        set => SetValue(AnimationEasingProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<int>? CurrentIndexChanged;

    #endregion

    protected override void OnParentSet()
    {
        base.OnParentSet();
        // Ensure initial layout is ready to show the first slide once we know our size
        if (Parent != null && !_initialized)
        {
            _initialized = true;
            SizeChanged += (_, __) => EnsureFirstSlideVisible();
            EnsureFirstSlideVisible();
        }
    }

    private void OnSlidesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Children.Clear();
            return;
        }

        if (e.OldItems != null)
        {
            foreach (View v in e.OldItems)
            {
                Children.Remove(v);
            }
        }

        if (e.NewItems != null)
        {
            foreach (View v in e.NewItems)
            {
                PrepareSlide(v);
                Children.Add(v);
            }
        }

        EnsureFirstSlideVisible();
    }

    private void PrepareSlide(View v)
    {
        v.IsVisible = false;
        v.TranslationX = 0;
    }

    private void EnsureFirstSlideVisible()
    {
        if (Slides.Count == 0 || Width <= 0) return;

        for (int i = 0; i < Slides.Count; i++)
        {
            var v = Slides[i];
            v.IsVisible = (i == CurrentIndex);
            v.TranslationX = 0;
        }
    }

    /// <summary>
    /// Move to the next slide if available.
    /// </summary>
    public Task NextAsync(bool animated = true) => GoToAsync(CurrentIndex + 1, animated);

    /// <summary>
    /// Move to the previous slide if available.
    /// </summary>
    public Task PreviousAsync(bool animated = true) => GoToAsync(CurrentIndex - 1, animated);

    /// <summary>
    /// Navigate to a specific slide by index.
    /// </summary>
    public async Task GoToAsync(int index, bool animated = true)
    {
        if (_isAnimating) return;
        if (index < 0 || index >= Slides.Count) return;
        if (Slides.Count == 0) return;

        int oldIndex = CurrentIndex;
        if (index == oldIndex)
            return;

        // Determine direction
        bool forward = index > oldIndex;

        var current = Slides[oldIndex];
        var next = Slides[index];

        // Ensure size is known
        double width = Width;
        if (width <= 0)
        {
            // Fallback: force layout if needed
            await this.Dispatcher.DispatchAsync(() => { InvalidateMeasure(); });
            width = Math.Max(Width, 1);
        }

        _isAnimating = true;

        next.IsVisible = true;
        next.TranslationX = forward ? width : -width;

        if (animated)
        {
            var t1 = current.TranslateTo(forward ? -width : width, 0, AnimationDuration, AnimationEasing);
            var t2 = next.TranslateTo(0, 0, AnimationDuration, AnimationEasing);
            await Task.WhenAll(t1, t2);
        }
        else
        {
            current.TranslationX = forward ? -width : width;
            next.TranslationX = 0;
        }

        current.IsVisible = false;
        current.TranslationX = 0; // reset off-screen view for reuse

        CurrentIndex = index; // triggers property changed (but we're already here); keep for binding coherence
        CurrentIndexChanged?.Invoke(this, index);

        _isAnimating = false;
    }

    /// <summary>
    /// Ensures CurrentIndex is within bounds and animates accordingly when changed via binding.
    /// </summary>
    private Task CoercedGoToAsync(int requestedIndex)
    {
        if (Slides.Count == 0) return Task.CompletedTask;

        int index = Math.Max(0, Math.Min(Slides.Count - 1, requestedIndex));

        // If we're already showing the desired index (e.g., during initialization), just ensure visibility.
        if (index == CurrentIndex && Slides[index].IsVisible)
        {
            EnsureFirstSlideVisible();
            return Task.CompletedTask;
        }

        // When CurrentIndex is set externally, animate to it.
        return GoToAsync(index, animated: true);
    }
}
