namespace SevexLabs.Ui.Maui.Controls
{
    public sealed class CountdownUpdatedEventArgs : EventArgs
    {
        public CountdownUpdatedEventArgs(double previousValue, double currentValue, double progress)
        {
            PreviousValue = previousValue;
            CurrentValue = currentValue;
            Progress = progress;
        }

        public double PreviousValue { get; }

        public double CurrentValue { get; }

        /// <summary>
        /// Progress normalized between 0 and 1.
        /// 1 means full, 0 means completed.
        /// </summary>
        public double Progress { get; }
    }
}
