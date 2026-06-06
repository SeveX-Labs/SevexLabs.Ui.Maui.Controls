using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// ShimmerLayout renders a shimmer effect over all BoxView descendants of its content,
/// respecting each BoxView's size, position and corner radius. The rest remains untouched.
/// The shimmer band is a single moving gradient sweeping across the whole container and
/// masked (clipped) by the union of those BoxViews.
/// </summary>
/// <remarks>
/// Animation resources are stopped automatically when the layout unloads or its handler detaches.
/// Do not expect overlay updates while the layout is detached.
/// </remarks>
[ContentProperty(nameof(Child))]
public class ShimmerLayout : ContentView
{
    private const string ShimmerAnimationName = "ShimmerLayoutAnim";

    public static readonly BindableProperty BackdropColorProperty =
        BindableProperty.Create(
            nameof(BackdropColor), typeof(Color), typeof(ShimmerLayout), Colors.Transparent,
            propertyChanged: OnVisualPropertyChanged);

    // Visual/animation properties (mirrors ShimmerLayout)
    public static readonly BindableProperty BackgroundGradientColorProperty =
        BindableProperty.Create(
            nameof(BackgroundGradientColor), typeof(Color), typeof(ShimmerLayout), Colors.LightGray,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShimmerGradientColorProperty =
        BindableProperty.Create(
            nameof(ShimmerGradientColor), typeof(Color), typeof(ShimmerLayout), Colors.White,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty AngleProperty =
        BindableProperty.Create(
            nameof(Angle), typeof(double), typeof(ShimmerLayout), -45d,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(
            nameof(Duration), typeof(uint), typeof(ShimmerLayout), 2000u,
            propertyChanged: OnDurationChanged);

    public static readonly BindableProperty GradientSizeProperty =
        BindableProperty.Create(
            nameof(GradientSize), typeof(double), typeof(ShimmerLayout), 0.2d,
            coerceValue: (_, v) => Math.Clamp((double)v, 0.02, 1.0),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty GradientSizeRelativeToWidthProperty =
        BindableProperty.Create(
            nameof(GradientSizeRelativeToWidth), typeof(bool), typeof(ShimmerLayout), false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShimmerOpacityProperty =
        BindableProperty.Create(
            nameof(ShimmerOpacity), typeof(double), typeof(ShimmerLayout), 0.5d,
            coerceValue: (_, v) => Math.Clamp((double)v, 0.0, 1.0),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius), typeof(double), typeof(ShimmerLayout), 0d,
            coerceValue: (_, v) => Math.Max(0d, (double)v),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading), typeof(bool), typeof(ShimmerLayout), true,
            propertyChanged: OnIsLoadingChanged);

    // The user content hosted inside the container
    public static readonly BindableProperty ChildProperty =
        BindableProperty.Create(
            nameof(Child), typeof(View), typeof(ShimmerLayout), default(View),
            propertyChanged: OnChildChanged);

    public Color BackdropColor
    {
        get => (Color)GetValue(BackdropColorProperty);
        set => SetValue(BackdropColorProperty, value);
    }

    public Color BackgroundGradientColor
    {
        get => (Color)GetValue(BackgroundGradientColorProperty);
        set => SetValue(BackgroundGradientColorProperty, value);
    }

    public Color ShimmerGradientColor
    {
        get => (Color)GetValue(ShimmerGradientColorProperty);
        set => SetValue(ShimmerGradientColorProperty, value);
    }

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of one shimmer pass, in milliseconds.
    /// </summary>
    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the bright band width, coerced to the supported range.
    /// </summary>
    public double GradientSize
    {
        get => (double)GetValue(GradientSizeProperty);
        set => SetValue(GradientSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether <see cref="GradientSize"/> is relative to the layout width instead of travel length.
    /// </summary>
    public bool GradientSizeRelativeToWidth
    {
        get => (bool)GetValue(GradientSizeRelativeToWidthProperty);
        set => SetValue(GradientSizeRelativeToWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity of the bright shimmer band.
    /// </summary>
    public double ShimmerOpacity
    {
        get => (double)GetValue(ShimmerOpacityProperty);
        set => SetValue(ShimmerOpacityProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the shimmer overlay is animated.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets the hosted content whose BoxView descendants receive the shimmer overlay.
    /// </summary>
    public View? Child
    {
        get => (View?)GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    private readonly Grid _root;
    private readonly Grid _contentHost;
    private readonly SKCanvasView _overlay;
    private double _progress; // 0..1
    private bool _isLoaded;

    public ShimmerLayout()
    {
        _root = new Grid();
        _contentHost = new Grid();
        _overlay = new SKCanvasView { IgnorePixelScaling = false, InputTransparent = true }; // sits above content

        BackgroundColor = Colors.Transparent;
        _root.BackgroundColor = Colors.Transparent;
        _contentHost.BackgroundColor = Colors.Transparent;
        _overlay.BackgroundColor = Colors.Transparent;

        _overlay.PaintSurface += OnPaintSurface;
        SizeChanged += (_, __) => TryInvalidateOverlaySurface();
        LayoutChanged += (_, __) => TryInvalidateOverlaySurface();

        // layering: content below, overlay on top
        _root.Children.Add(_contentHost);
        _root.Children.Add(_overlay);
        base.Content = _root;

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    private static void OnChildChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ShimmerLayout sc) return;
        sc._contentHost.Children.Clear();
        if (newValue is View v)
        {
            sc._contentHost.Children.Add(v);
        }
        sc.TryInvalidateOverlaySurface();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerLayout sc)
            sc.TryInvalidateOverlaySurface();
    }

    private static void OnDurationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerLayout sc)
        {
            if (sc.IsLoading)
            {
                sc.StopCore(invalidateSurface: false);
                sc.TryStart();
            }
            else
            {
                sc.TryInvalidateOverlaySurface();
            }
        }
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerLayout sc)
        {
            if (sc.IsLoading) sc.TryStart();
            else sc.Stop();
        }
    }

    private void TryStart()
    {
        if (!IsLoading)
        {
            TryInvalidateOverlaySurface();
            return;
        }

        if (!_isLoaded || Handler is null)
        {
            TryInvalidateOverlaySurface();
            return;
        }

        this.AbortAnimation(ShimmerAnimationName);

        new Animation(p =>
        {
            _progress = p;
            TryInvalidateOverlaySurface();
        }, 0, 1)
        .Commit(this, ShimmerAnimationName, rate: 32, length: Duration, easing: Easing.Linear,
            finished: (v, c) => { _progress = 0; },
            repeat: () => IsLoading && _isLoaded && Handler is not null);
    }

    private void Stop()
    {
        StopCore(invalidateSurface: true);
    }

    private void StopCore(bool invalidateSurface)
    {
        this.AbortAnimation(ShimmerAnimationName);
        _progress = 0;

        if (invalidateSurface)
        {
            TryInvalidateOverlaySurface();
        }
    }

    private void TryInvalidateOverlaySurface()
    {
        if (_overlay.Handler?.PlatformView is not null)
        {
            _overlay.InvalidateSurface();
        }
    }

    private void HandleLoaded(object? sender, EventArgs e)
    {
        _isLoaded = true;
        TryStart();
    }

    private void HandleUnloaded(object? sender, EventArgs e)
    {
        _isLoaded = false;
        StopCore(invalidateSurface: true);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            TryStart();
        }
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.OldHandler is not null)
        {
            StopCore(invalidateSurface: false);
        }

        base.OnHandlerChanging(args);
    }

    private sealed record BoxInfo(SKRect RectPx, float RadiusPx);

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        float w = info.Width;
        float h = info.Height;

        canvas.Clear(BackdropColor.ToSKColor());

        // Prendi l’unione dei BoxView (in pixel)
        var boxes = CollectBoxRectsPx(e);
        if (boxes.Count == 0)
            return;

        using var boxesPath = new SKPath();
        foreach (var b in boxes)
            boxesPath.AddRoundRect(new SKRoundRect(b.RectPx, b.RadiusPx, b.RadiusPx));

        // 1) Riempimento base SOLO dentro il path (niente clip!)
        using (var bg = new SKPaint { Color = BackgroundGradientColor.ToSKColor(), IsAntialias = true })
            canvas.DrawPath(boxesPath, bg);

        // 2) Banda shimmer: shader orizzontale + rotazione via local matrix (non ruotiamo il canvas)
        if (IsLoading)
        {
            var angRad = (float)(Math.PI * Angle / 180.0);
            var cosA = MathF.Abs(MathF.Cos(angRad));
            var sinA = MathF.Abs(MathF.Sin(angRad));

            var travel = w * cosA + h * sinA;
            var bandWidth = GradientSizeRelativeToWidth
                ? MathF.Max(1f, (float)GradientSize * w)
                : MathF.Max(1f, (float)GradientSize * travel);

            var startX = (w - travel) / 2f - bandWidth;
            var endX = (w + travel) / 2f + bandWidth;
            var centerX = startX + (endX - startX) * (float)_progress;

            var alpha = (byte)Math.Clamp((int)Math.Round(255 * ShimmerOpacity), 0, 255);
            var c0 = ShimmerGradientColor.WithAlpha(0).ToSKColor();
            var c1 = ShimmerGradientColor.WithAlpha(alpha).ToSKColor();

            using var paint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High,
                BlendMode = SKBlendMode.Screen
            };

            // Gradient “stretto” centrato sulla banda (fuori banda alpha=0)
            var shader = SKShader.CreateLinearGradient(
                new SKPoint(centerX - bandWidth / 2f, 0),
                new SKPoint(centerX + bandWidth / 2f, 0),
                new[] { c0, c1, c1, c0 },
                new[] { 0f, 0.40f, 0.60f, 1f },
                SKShaderTileMode.Clamp);

            // Ruota lo shader attorno al centro del canvas
            var m = SKMatrix.CreateRotationDegrees((float)Angle, w / 2f, h / 2f);
            paint.Shader = shader.WithLocalMatrix(m);

            // Disegna SOLO il path dei BoxView
            canvas.DrawPath(boxesPath, paint);
        }

        // 3) CornerRadius esterno del contenitore: usa una maschera (DstIn), NON una clip
        if (CornerRadius > 0)
        {
            var r = (float)Math.Min(CornerRadius, Math.Min(w, h) / 2f);
            using var maskPaint = new SKPaint { IsAntialias = true, BlendMode = SKBlendMode.DstIn };
            using var rr = new SKRoundRect(new SKRect(0, 0, w, h), r, r);
            using var outerMask = new SKPath();
            outerMask.AddRoundRect(rr);
            canvas.DrawPath(outerMask, maskPaint);
        }
    }

    private List<BoxInfo> CollectBoxRectsPx(SKPaintSurfaceEventArgs e)
    {
        var list = new List<BoxInfo>();
        if (_contentHost.Children.Count == 0)
            return list;

        var root = _contentHost.Children[0] as VisualElement;
        if (root == null)
            return list;

        float w = e.Info.Width;
        float h = e.Info.Height;
        // Convert from DIPs to pixels
        float scaleX = (float)(w / Width);
        float scaleY = (float)(h / Height);
        float radiusScale = MathF.Min(scaleX, scaleY);

        void Visit(VisualElement ve)
        {
            if (ve is BoxView box)
            {
                // Box bounds in DIPs
                float x = (float)box.X;
                float y = (float)box.Y;
                float bw = (float)box.Width;
                float bh = (float)box.Height;

                if (bw > 0 && bh > 0)
                {
                    var rectPx = new SKRect(x * scaleX, y * scaleY, (x + bw) * scaleX, (y + bh) * scaleY);
                    float rPx = GetCornerRadiusPx(box, radiusScale);
                    list.Add(new BoxInfo(rectPx, rPx));
                }
            }

            if (ve is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is VisualElement cve) Visit(cve);
                }
            }
            else if (ve is ContentView cv && cv.Content is VisualElement cvc)
            {
                Visit(cvc);
            }
            else if (ve is ScrollView sv && sv.Content is VisualElement svc)
            {
                Visit(svc);
            }
        }

        Visit(root);
        return list;
    }

    private static float GetCornerRadiusPx(BoxView box, float scale)
    {
        try
        {
            var prop = typeof(BoxView).GetProperty("CornerRadius");
            if (prop != null)
            {
                var val = prop.GetValue(box);
                if (val is CornerRadius cr)
                {
                    var r = Math.Max(Math.Max(cr.TopLeft, cr.TopRight), Math.Max(cr.BottomLeft, cr.BottomRight));
                    return (float)r * scale;
                }
                if (val is double d)
                {
                    return (float)d * scale;
                }
            }
        }
        catch { }
        return 0f;
    }
}
