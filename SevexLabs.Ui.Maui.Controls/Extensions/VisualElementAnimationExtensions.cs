namespace SevexLabs.Ui.Maui.Controls.Extensions
{
    internal static class VisualElementAnimationExtensions
    {
        public static Task AnimateAsync(
            this VisualElement view,
            string name,
            Action<double> callback,
            double start,
            double end,
            uint length,
            Easing? easing = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            var animation = new Animation(callback, start, end, easing ?? Easing.Linear);

            animation.Commit(
                owner: view,
                name: name,
                rate: 16,
                length: length,
                finished: (_, _) => tcs.TrySetResult(true));

            return tcs.Task;
        }
    }
}
