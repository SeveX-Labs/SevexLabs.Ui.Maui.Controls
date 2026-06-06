namespace SevexLabs.Ui.Maui.Controls
{
    internal sealed class ArcCountdownDrawable : IDrawable
    {
        private readonly ArcCountdown _owner;

        public ArcCountdownDrawable(ArcCountdown owner)
        {
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Antialias = true;

            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);

            if (size <= 0)
            {
                canvas.RestoreState();
                return;
            }

            var centerX = dirtyRect.Center.X;
            var centerY = dirtyRect.Center.Y;

            var outerStrokeThickness = Math.Max(1f, (float)_owner.TrackThickness);
            var outerRadius = (size / 2f) - (outerStrokeThickness / 2f);

            if (outerRadius <= 0)
            {
                canvas.RestoreState();
                return;
            }

            var innerArcThickness = Math.Max(0f, (float)_owner.InnerArcThickness);
            var innerArcOuterMargin = Math.Max(0f, (float)_owner.InnerArcOuterMargin);
            var innerArcInnerMargin = Math.Max(0f, (float)_owner.InnerArcInnerMargin);

            var outerRingInnerEdgeRadius = Math.Max(0f, outerRadius - (outerStrokeThickness / 2f));
            var innerArcRadius = outerRingInnerEdgeRadius - innerArcOuterMargin - (innerArcThickness / 2f);
            var centerRadius = outerRingInnerEdgeRadius - innerArcOuterMargin - innerArcThickness - innerArcInnerMargin;

            DrawCenterBackground(canvas, centerX, centerY, centerRadius);
            DrawInnerArc(canvas, centerX, centerY, innerArcRadius, innerArcThickness);
            DrawOuterShadow(canvas, centerX, centerY, outerRadius, outerStrokeThickness);
            DrawTrack(canvas, centerX, centerY, outerRadius, outerStrokeThickness);
            DrawProgress(canvas, centerX, centerY, outerRadius, outerStrokeThickness);
            DrawText(canvas, dirtyRect);

            canvas.RestoreState();
        }

        private void DrawCenterBackground(ICanvas canvas, float centerX, float centerY, float centerRadius)
        {
            var clampedRadius = Math.Max(0f, centerRadius);
            var diameter = clampedRadius * 2f;

            if (diameter <= 0f)
            {
                return;
            }

            canvas.FillColor = _owner.CenterBackgroundColor;
            canvas.FillEllipse(centerX - clampedRadius, centerY - clampedRadius, diameter, diameter);
        }

        private void DrawInnerArc(ICanvas canvas, float centerX, float centerY, float radius, float strokeThickness)
        {
            if (strokeThickness <= 0f || radius <= 0f)
            {
                return;
            }

            canvas.StrokeColor = _owner.InnerArcColor;
            canvas.StrokeSize = strokeThickness;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawCircle(centerX, centerY, radius);
        }

        private void DrawOuterShadow(ICanvas canvas, float centerX, float centerY, float trackRadius, float trackStrokeThickness)
        {
            if (!_owner.TrackUseShadow || _owner.TrackShadow is null)
            {
                return;
            }

            if (!TryGetShadowColor(_owner.TrackShadow, out var shadowColor))
            {
                return;
            }

            var blurRadius = Math.Max(0f, (float)_owner.TrackShadow.Radius);
            if (blurRadius <= 0f || shadowColor.Alpha <= 0f)
            {
                return;
            }

            var offsetX = (float)_owner.TrackShadow.Offset.X;
            var offsetY = (float)_owner.TrackShadow.Offset.Y;

            // Bordo esterno reale del track disegnato.
            var trackOuterEdgeRadius = trackRadius + (trackStrokeThickness / 2f);

            // Più layer = blur più morbido.
            var layerCount = Math.Clamp((int)MathF.Ceiling(blurRadius * 1.5f), 6, 24);

            canvas.SaveState();
            canvas.StrokeLineCap = LineCap.Butt;

            for (int i = 0; i < layerCount; i++)
            {
                var t = (i + 1f) / layerCount;

                // Spessore del layer shadow.
                // Può essere sottile all'inizio e un po' più pieno andando fuori.
                var layerStrokeThickness = Math.Max(1f, blurRadius * 0.9f);

                // Fondamentale:
                // il bordo INTERNO del layer deve stare FUORI dal bordo esterno del track.
                //
                // innerEdge = layerRadius - layerStrokeThickness / 2
                // imponiamo: innerEdge = trackOuterEdgeRadius + extraOutside
                var extraOutside = blurRadius * t;
                var layerRadius = trackOuterEdgeRadius + extraOutside + (layerStrokeThickness / 2f);

                // Decadimento alpha.
                var alphaFactor = 1f - t;
                alphaFactor *= alphaFactor;

                var layerAlpha = shadowColor.Alpha * alphaFactor * 0.42f;
                if (layerAlpha <= 0.002f)
                {
                    continue;
                }

                canvas.StrokeColor = new Color(
                    shadowColor.Red,
                    shadowColor.Green,
                    shadowColor.Blue,
                    layerAlpha);

                canvas.StrokeSize = layerStrokeThickness;
                canvas.DrawCircle(centerX + offsetX, centerY + offsetY, layerRadius);
            }

            canvas.RestoreState();
        }

        private void DrawTrack(ICanvas canvas, float centerX, float centerY, float radius, float strokeThickness)
        {
            canvas.StrokeColor = _owner.TrackColor;
            canvas.StrokeSize = strokeThickness;
            canvas.StrokeLineCap = MapTrackStrokeCap(_owner.StrokeCap);
            canvas.DrawCircle(centerX, centerY, radius);
        }

        private void DrawProgress(ICanvas canvas, float centerX, float centerY, float radius, float strokeThickness)
        {
            var progress = Math.Max(0d, Math.Min(1d, _owner.DisplayedProgress));

            if (progress <= 0d)
            {
                return;
            }

            canvas.StrokeColor = _owner.ProgressColor;
            canvas.StrokeSize = strokeThickness;

            if (progress >= 0.999999d)
            {
                canvas.StrokeLineCap = MapTrackStrokeCap(_owner.StrokeCap);
                canvas.DrawCircle(centerX, centerY, radius);
                return;
            }

            const float originalStartAngleDegrees = -90f;
            var originalSweepAngleDegrees = (float)(360d * progress);
            var directionSign = _owner.Direction == CountdownDirection.Clockwise ? -1f : 1f;

            if (originalSweepAngleDegrees <= 0f)
            {
                return;
            }

            var requestedCap = _owner.StrokeCap;

            if (requestedCap == ArcStrokeCap.RoundInline)
            {
                DrawInlineCap(
                    canvas,
                    centerX,
                    centerY,
                    radius,
                    strokeThickness,
                    originalStartAngleDegrees,
                    originalSweepAngleDegrees,
                    directionSign,
                    LineCap.Round);

                return;
            }

            DrawArcPath(
                canvas,
                centerX,
                centerY,
                radius,
                originalStartAngleDegrees,
                originalSweepAngleDegrees,
                directionSign,
                MapProgressStrokeCap(requestedCap),
                strokeThickness);
        }

        private void DrawInlineCap(
            ICanvas canvas,
            float centerX,
            float centerY,
            float radius,
            float strokeThickness,
            float originalStartAngleDegrees,
            float originalSweepAngleDegrees,
            float directionSign,
            LineCap cap)
        {
            var capAngleDegrees = RadiansToDegrees((strokeThickness / 2f) / radius);
            var minimumSweep = capAngleDegrees * 2f;

            if (originalSweepAngleDegrees <= minimumSweep)
            {
                DrawArcPath(
                    canvas,
                    centerX,
                    centerY,
                    radius,
                    originalStartAngleDegrees,
                    originalSweepAngleDegrees,
                    directionSign,
                    LineCap.Butt,
                    strokeThickness);

                return;
            }

            var compensatedStart = originalStartAngleDegrees + (capAngleDegrees * directionSign);
            var compensatedSweep = originalSweepAngleDegrees - minimumSweep;

            DrawArcPath(
                canvas,
                centerX,
                centerY,
                radius,
                compensatedStart,
                compensatedSweep,
                directionSign,
                cap,
                strokeThickness);
        }

        private void DrawArcPath(
            ICanvas canvas,
            float centerX,
            float centerY,
            float radius,
            float startAngleDegrees,
            float sweepAngleDegrees,
            float directionSign,
            LineCap lineCap,
            float strokeThickness)
        {
            var signedSweep = sweepAngleDegrees * directionSign;

            if (sweepAngleDegrees <= 0f)
            {
                return;
            }

            canvas.StrokeLineCap = lineCap;

            var segments = CalculateArcSegments(radius, strokeThickness, sweepAngleDegrees);
            var path = new PathF();

            for (int i = 0; i <= segments; i++)
            {
                var t = (float)i / segments;
                var angle = startAngleDegrees + (signedSweep * t);
                var radians = DegreesToRadians(angle);

                var x = centerX + (radius * MathF.Cos(radians));
                var y = centerY + (radius * MathF.Sin(radians));

                if (i == 0)
                {
                    path.MoveTo(x, y);
                }
                else
                {
                    path.LineTo(x, y);
                }
            }

            canvas.DrawPath(path);
        }

        private static int CalculateArcSegments(float radius, float strokeThickness, float sweepAngleDegrees)
        {
            if (radius <= 0f || sweepAngleDegrees <= 0f)
                return 8;

            var circumference = 2f * MathF.PI * radius;
            var arcLength = circumference * (sweepAngleDegrees / 360f);
            var targetSegmentLength = Math.Max(1.25f, strokeThickness * 0.18f);
            var segments = (int)MathF.Ceiling(arcLength / targetSegmentLength);

            return Math.Clamp(segments, 24, 720);
        }

        private void DrawText(ICanvas canvas, RectF dirtyRect)
        {
            var text = _owner.GetFormattedDisplayText();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            canvas.SaveState();

            ApplyTextShadow(canvas);

            canvas.FontColor = _owner.TextColor;
            canvas.FontSize = (float)_owner.FontSize;

            if (!string.IsNullOrWhiteSpace(_owner.FontFamily))
            {
                canvas.Font = new Microsoft.Maui.Graphics.Font(_owner.FontFamily!);
            }

            canvas.DrawString(
                text,
                dirtyRect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);

            canvas.RestoreState();
        }

        private void ApplyTextShadow(ICanvas canvas)
        {
            if (!_owner.TextUseShadow || _owner.TextShadow is null)
            {
                return;
            }

            if (!TryGetShadowColor(_owner.TextShadow, out var shadowColor))
            {
                return;
            }

            var offset = new SizeF(
                (float)_owner.TextShadow.Offset.X,
                (float)_owner.TextShadow.Offset.Y);

            var blur = (float)Math.Max(0d, _owner.TextShadow.Radius);

            canvas.SetShadow(offset, blur, shadowColor);
        }

        private static bool TryGetShadowColor(Shadow shadow, out Color color)
        {
            if (shadow.Brush is SolidColorBrush solidColorBrush)
            {
                color = solidColorBrush.Color;
                return true;
            }

            color = Colors.Transparent;
            return false;
        }

        private static float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }

        private static float RadiansToDegrees(float radians)
        {
            return radians * (180f / MathF.PI);
        }

        private static LineCap MapTrackStrokeCap(ArcStrokeCap strokeCap)
        {
            return strokeCap switch
            {
                ArcStrokeCap.Butt => LineCap.Butt,
                ArcStrokeCap.Square => LineCap.Square,
                ArcStrokeCap.RoundInline => LineCap.Round,
                _ => LineCap.Round
            };
        }

        private static LineCap MapProgressStrokeCap(ArcStrokeCap strokeCap)
        {
            return strokeCap switch
            {
                ArcStrokeCap.Butt => LineCap.Butt,
                ArcStrokeCap.Square => LineCap.Square,
                ArcStrokeCap.Round => LineCap.Round,
                _ => LineCap.Butt
            };
        }
    }
}