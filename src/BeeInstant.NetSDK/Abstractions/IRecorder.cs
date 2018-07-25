namespace BeeInstant.NetSDK.Abstractions
{
    public interface IRecorder : IMetric<IRecorder>
    {
        void Record(double value, Unit unit);
    }
}