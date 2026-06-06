namespace SevexLabs.Ui.Maui.Controls
{
    public sealed class PopCountdownValueChangedEventArgs : EventArgs
    {
        public PopCountdownValueChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int OldValue { get; }
        public int NewValue { get; }
    }
}
