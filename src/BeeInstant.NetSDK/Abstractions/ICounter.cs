namespace BeeInstant.NetSDK.Abstractions
{
    public interface ICounter : IMetric<ICounter>
    {
        void IncrementCounter(long value);

        long GetValue();

        void Reset();
    }
}