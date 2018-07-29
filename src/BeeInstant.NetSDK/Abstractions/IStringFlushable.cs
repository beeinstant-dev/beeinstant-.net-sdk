using System;

namespace BeeInstant.NetSDK.Abstractions
{
    public interface IStringFlushable
    {
        string FlushToString();
    }

    public interface IStringFlushableByAction : IStringFlushable
    {
        string FlushToString(Action<string> action);
    }
}