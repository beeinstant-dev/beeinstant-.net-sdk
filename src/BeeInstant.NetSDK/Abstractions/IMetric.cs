namespace BeeInstant.NetSDK.Abstractions
{
    public interface IMetric<T> : IMetric
    {
        T Merge(T target);
    }

    public interface IMetric
    {
        IMetric Merge(IMetric target);
        string FlushToString();
    }
}