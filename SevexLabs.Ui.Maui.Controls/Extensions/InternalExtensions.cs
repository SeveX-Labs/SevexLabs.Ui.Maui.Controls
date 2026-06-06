namespace SevexLabs.Ui.Maui.Controls.Extensions
{
    internal static class InternalExtensions
    {
        public static bool SequenceEqualShallow(this IList<object>? a, IList<object>? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null || a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (!Equals(a[i], b[i])) return false;
            return true;
        }
    }
}
