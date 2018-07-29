namespace BeeInstant.NetSDK.Abstractions
{
    public interface IMetric<T> : IMetric
    {
        T Merge(T target);
    }

    public interface IMetric : IStringFlushable
    {
        IMetric Merge(IMetric target);
    }
}