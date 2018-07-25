namespace BeeInstant.NetSDK.Abstractions
{
    public interface ITimer : IMetric<ITimer>
    {
        long StartTimer();

        void StopTimer(long startTime);
    }
}