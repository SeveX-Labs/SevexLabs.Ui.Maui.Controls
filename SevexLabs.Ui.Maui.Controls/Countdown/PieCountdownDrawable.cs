namespace SevexLabs.Ui.Maui.Controls
{
    internal sealed class PieCountdownDrawable : IDrawable
    {
        private readonly PieCountdown _owner;

        public PieCountdownDrawable(PieCountdown owner)
        {
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Antialias = true;

            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);

            // The pie must remain circular, so drawing is constrained to the smallest dimension.
            if (size <= 0)
            {
                canvas.RestoreState();
                return;
            }

            var centerX = dirtyRect.Center.X;
            var centerY = dirtyRect.Center.Y;
            var radius = size / 2f;

            if (radius <= 0)
            {
                canvas.RestoreState();
                return;
            }

            DrawBackground(canvas, centerX, centerY, radius);
            DrawProgress(canvas, centerX, centerY, radius);
            DrawText(canvas, dirtyRect);

            canvas.RestoreState();
        }

        private void DrawBackground(ICanvas canvas, float centerX, float centerY, float radius)
        {
            // The background represents the full circular area behind the active progress sector.
            canvas.FillColor = _owner.CircleBackgroundColor;
            canvas.FillCircle(centerX, centerY, radius);
        }

        private void DrawProgress(ICanvas canvas, float centerX, float centerY, float radius)
        {
            // The displayed progress is clamped to avoid invalid geometry when the control is animated
            // or updated with values slightly outside the expected range.
            var progress = Math.Max(0d, Math.Min(1d, _owner.DisplayedProgress));

            if (progress <= 0d)
            {
                return;
            }

            canvas.FillColor = _owner.FillColor;

            // A nearly complete progress is promoted to a full circle to prevent a thin seam
            // caused by floating-point precision at the arc closure point.
            if (progress >= 0.999999d)
            {
                canvas.FillCircle(centerX, centerY, radius);
                return;
            }

            const float startAngleDegrees = -90f;
            var sweepAngleDegrees = (float)(360d * progress);

            // Sign inversion is used to match the expected clockwise/counterclockwise behavior
            // of the control within the screen coordinate system.
            var directionSign = _owner.Direction == CountdownDirection.Clockwise ? -1f : 1f;
            var signedSweep = sweepAngleDegrees * directionSign;

            var path = new PathF();

            // The sector starts from the center, walks along the circular boundary,
            // and is then closed back to the center to produce a filled pie slice.
            path.MoveTo(centerX, centerY);

            // A fixed number of segments is used here because the pie is filled, not stroked:
            // the visual artifacts are typically less evident than in outline-based rendering.
            const int segments = 180;

            for (int i = 0; i <= segments; i++)
            {
                var t = (float)i / segments;
                var angle = startAngleDegrees + (signedSweep * t);
                var radians = DegreesToRadians(angle);

                var x = centerX + (radius * MathF.Cos(radians));
                var y = centerY + (radius * MathF.Sin(radians));

                path.LineTo(x, y);
            }

            path.Close();

            canvas.FillPath(path);
        }

        private static float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }

        private void DrawText(ICanvas canvas, RectF dirtyRect)
        {
            var text = _owner.GetFormattedDisplayText();

            // Empty text is intentionally ignored so the countdown can be rendered as a pure shape.
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            canvas.FontColor = _owner.TextColor;
            canvas.FontSize = (float)_owner.FontSize;

            // The font family is applied only when explicitly configured,
            // otherwise the canvas default font is preserved.
            if (!string.IsNullOrWhiteSpace(_owner.FontFamily))
            {
                canvas.Font = new Microsoft.Maui.Graphics.Font(_owner.FontFamily!);
            }

            canvas.DrawString(
                text,
                dirtyRect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }
}