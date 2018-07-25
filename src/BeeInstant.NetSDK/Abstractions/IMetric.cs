namespace BeeInstant.NetSDK.Abstractions
{
    public interface IMetric<T>
    {
        string FlushToString();

        T Merge(T target);
    }
}