namespace SevexLabs.Ui.Maui.Controls
{
    public sealed class PopCountdownDisplayTextChangedEventArgs : EventArgs
    {
        public PopCountdownDisplayTextChangedEventArgs(string? oldText, string? newText)
        {
            OldText = oldText;
            NewText = newText;
        }

        public string? OldText { get; }
        public string? NewText { get; }
    }
}
