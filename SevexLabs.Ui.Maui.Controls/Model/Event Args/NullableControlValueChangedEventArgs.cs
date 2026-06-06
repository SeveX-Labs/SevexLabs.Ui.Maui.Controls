namespace SevexLabs.Ui.Maui.Controls.Model.Event_Args
{
    public class NullableControlValueChangedEventArgs : EventArgs
    {
        public NullableControlValueChangedEventArgs(object? source, double? oldValue, double? newValue)
        {
            Source = source;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object? Source { get; private set; }

        public double? NewValue { get; private set; }

        public double? OldValue { get; private set; }
    }
}
