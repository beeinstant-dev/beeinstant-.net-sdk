namespace BeeInstant.NetSDK.Abstractions
{
    public interface IMetricsComposer 
    {
        void IncrementCounter(string counterName, int value);

        TimerMetric StartTimer(string timerName);

        void StopTimer(string timerName);

        void Record(string metricName, decimal value, Unit unit);
    }
    
    public interface IMetricsComposer<T> : IMetric<T>, IMetricsComposer
    {

    }
}