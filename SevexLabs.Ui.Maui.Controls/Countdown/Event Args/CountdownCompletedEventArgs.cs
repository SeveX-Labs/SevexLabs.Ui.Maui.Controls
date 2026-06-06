namespace SevexLabs.Ui.Maui.Controls
{
    public sealed class CountdownCompletedEventArgs : EventArgs
    {
        public CountdownCompletedEventArgs(double finalValue)
        {
            FinalValue = finalValue;
        }

        public double FinalValue { get; }
    }
}
