namespace BeeInstant.NetSDK.Abstractions
{
    public interface ITimer : IMetric<ITimer>
    {
        void StartTimer();

        void StopTimer();
    }
}