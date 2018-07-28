namespace BeeInstant.NetSDK.Abstractions
{
    public interface IRecorder : IMetric<IRecorder>
    {
        void Record(decimal value, Unit unit);
    }
}