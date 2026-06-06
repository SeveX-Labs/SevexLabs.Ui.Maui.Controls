using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SevexLabs.Ui.Maui.Controls;

/// <summary>
/// A lightweight shimmer/skeleton placeholder implemented with SkiaSharp.
/// Designed to render a single shimmering block (no child layout hosting).
/// </summary>
/// <remarks>
/// Animation resources are stopped automatically when the view unloads or its handler detaches.
/// Do not expect visual updates while the view is detached.
/// </remarks>
public class ShimmerView : SKCanvasView
{
    private const string ShimmerAnimationName = "ShimmerAnim";

    public static readonly BindableProperty BackgroundGradientColorProperty =
        BindableProperty.Create(
            nameof(BackgroundGradientColor),
            typeof(Color),
            typeof(ShimmerView),
            Colors.LightGray,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShimmerGradientColorProperty =
        BindableProperty.Create(
            nameof(ShimmerGradientColor),
            typeof(Color),
            typeof(ShimmerView),
            Colors.White,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty AngleProperty =
        BindableProperty.Create(
            nameof(Angle),
            typeof(double),
            typeof(ShimmerView),
            -45d,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(
            nameof(Duration),
            typeof(uint),
            typeof(ShimmerView),
            1000u,
            propertyChanged: OnDurationChanged);

    /// <summary>
    /// Width of the bright band relative to the travel length (0..1).
    /// </summary>
    public static readonly BindableProperty GradientSizeProperty =
        BindableProperty.Create(
            nameof(GradientSize),
            typeof(double),
            typeof(ShimmerView),
            0.2d,
            coerceValue: (_, v) => Math.Clamp((double)v, 0.02, 1.0),
            propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// If true, GradientSize is relative to the view width; otherwise it's relative to the travel length.
    /// </summary>
    public static readonly BindableProperty GradientSizeRelativeToWidthProperty =
        BindableProperty.Create(
            nameof(GradientSizeRelativeToWidth),
            typeof(bool),
            typeof(ShimmerView),
            false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShimmerOpacityProperty =
        BindableProperty.Create(
            nameof(ShimmerOpacity),
            typeof(double),
            typeof(ShimmerView),
            0.5d,
            coerceValue: (_, v) => Math.Clamp((double)v, 0.0, 1.0),
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(ShimmerView),
            true,
            propertyChanged: OnIsLoadingChanged);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(double),
            typeof(ShimmerView),
            0d,
            coerceValue: (_, v) => Math.Max(0d, (double)v),
            propertyChanged: OnVisualPropertyChanged);

    private double _progress; // 0..1 position along the travel path
    private bool _isLoaded;

    public ShimmerView()
    {
        EnableTouchEvents = false;
        PaintSurface += OnPaintSurface;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
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

    /// <summary>
    /// Angle of the shimmer movement in degrees. 0 moves left→right, -45 is typical.
    /// </summary>
    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <summary>
    /// Duration of one shimmer pass, in milliseconds.
    /// </summary>
    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Bright band width relative to travel length, coerced to the supported range.
    /// </summary>
    public double GradientSize
    {
        get => (double)GetValue(GradientSizeProperty);
        set => SetValue(GradientSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether <see cref="GradientSize"/> is relative to the view width instead of travel length.
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

    /// <summary>
    /// Starts or stops the shimmer. When false, shows the static background.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Uniform corner radius in device-independent units.
    /// </summary>
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerView v)
            v.TryInvalidateSurface();
    }

    private static void OnDurationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerView v)
        {
            // Restart animation to apply new duration
            if (v.IsLoading)
            {
                v.StopCore(invalidateSurface: false);
                v.TryStart();
            }
            else
            {
                v.TryInvalidateSurface();
            }
        }
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerView v)
        {
            if (v.IsLoading) v.TryStart();
            else v.Stop();
        }
    }

    private void TryStart()
    {
        if (!IsLoading)
        {
            TryInvalidateSurface();
            return;
        }

        if (!_isLoaded || Handler is null)
        {
            TryInvalidateSurface();
            return;
        }

        this.AbortAnimation(ShimmerAnimationName);

        // A single, self-restarting animation loop
        new Animation(p =>
        {
            _progress = p; // 0..1
            TryInvalidateSurface();
        }, 0, 1)
        .Commit(
            this,
            ShimmerAnimationName,
            rate: 16,
            length: Duration,
            easing: Easing.Linear,
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
            TryInvalidateSurface();
        }
    }

    private void TryInvalidateSurface()
    {
        if (Handler?.PlatformView is not null)
        {
            InvalidateSurface();
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

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var w = (float)info.Width;
        var h = (float)info.Height;

        canvas.Clear();

        // Optional rounded-rect clipping
        bool clipped = false;
        var radius = (float)Math.Max(0, CornerRadius);
        if (radius > 0f)
        {
            radius = MathF.Min(radius, MathF.Min(w, h) / 2f);
            using var clipPath = new SKPath();
            var rr = new SKRoundRect(new SKRect(0, 0, w, h), radius, radius);
            clipPath.AddRoundRect(rr);
            canvas.Save();
            canvas.ClipPath(clipPath, antialias: true);
            clipped = true;
        }

        // Fill base background
        using (var bg = new SKPaint { Color = BackgroundGradientColor.ToSKColor(), IsAntialias = true })
        {
            canvas.DrawRect(0, 0, w, h, bg);
        }

        if (!IsLoading)
        {
            if (clipped) canvas.Restore();
            return;
        }

        // Compute travel length for the shimmer along the chosen angle
        var angRad = (float)(Math.PI * Angle / 180.0);
        var cosA = MathF.Abs(MathF.Cos(angRad));
        var sinA = MathF.Abs(MathF.Sin(angRad));

        // Effective travel distance across the rotated rectangle
        var travel = w * cosA + h * sinA;              // how far the band needs to move across view
        var bandWidth = GradientSizeRelativeToWidth
            ? MathF.Max(1f, (float)GradientSize * w)
            : MathF.Max(1f, (float)GradientSize * travel);

        var startX = (w - travel) / 2f - bandWidth;    // start just outside
        var endX = (w + travel) / 2f + bandWidth;    // end just outside
        var centerX = startX + (endX - startX) * (float)_progress;

        // Rotate canvas to align X with the shimmer direction
        canvas.Save();
        canvas.Translate(w / 2f, h / 2f);
        canvas.RotateDegrees((float)Angle);
        canvas.Translate(-w / 2f, -h / 2f);

        // Ensure the band covers the full container height regardless of angle
        var span = w * sinA + h * cosA; // perpendicular span across the rotated rectangle
        var yCenter = h / 2f;
        var rect = new SKRect(
            centerX - bandWidth / 2f,
            yCenter - span / 2f - bandWidth,  // a little overscan to avoid edge clipping
            centerX + bandWidth / 2f,
            yCenter + span / 2f + bandWidth
        );
        // var rect = new SKRect(centerX - bandWidth / 2f, 0, centerX + bandWidth / 2f, h);

        // Build a soft band: transparent -> bright -> transparent
        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            BlendMode = SKBlendMode.Screen // lighten the base nicely
        };

        var c0 = BackgroundGradientColor.WithAlpha(0).ToSKColor();

        var alpha = (byte)Math.Clamp((int)Math.Round(255 * ShimmerOpacity), 0, 255);
        // var alphaSoft = (byte)(Math.Clamp((int)Math.Round(255 * ShimmerOpacity), 0, 255) * 0.6);
        var c1 = ShimmerGradientColor.WithAlpha(alpha).ToSKColor();
        // var c1Soft = ShimmerGradientColor.WithAlpha(alphaSoft).ToSKColor();


        /*
        paint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(rect.Left, 0),
            new SKPoint(rect.Right, 0),
            new[] { c0, c1, c2 },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);
        */
        paint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(rect.Left, 0),
            new SKPoint(rect.Right, 0),
            new[] { c0, c1, c1, c0 },
            new[] { 0f, 0.4f, 0.6f, 1f },
            SKShaderTileMode.Clamp);
        /*
        paint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(rect.Left, 0),
            new SKPoint(rect.Right, 0),
            new[] { c0, c1Soft, c1, c1Soft, c0 },
            new[] { 0f, 0.30f, 0.50f, 0.70f, 1f },
            SKShaderTileMode.Clamp);
            */

        canvas.DrawRect(rect, paint);
        canvas.Restore();

        if (clipped) canvas.Restore(); // undo clipping
    }
}
