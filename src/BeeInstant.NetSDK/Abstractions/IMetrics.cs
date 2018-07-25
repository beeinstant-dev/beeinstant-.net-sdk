namespace BeeInstant.NetSDK.Abstractions
{
    public interface IMetrics
    {
        void IncrementCounter(string counterName, int value);

        //TODO: Add TimerMetric
        //TimerMetric StartTimer(string timerName);

        void Record(string metricName, double value, Unit unit);
    }
}