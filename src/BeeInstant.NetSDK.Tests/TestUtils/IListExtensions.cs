using System;
using System.Collections.Generic;
using System.Linq;

namespace BeeInstant.NetSDK.Tests.Utils
{
    public static class IListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            list = list.OrderBy(_ => new Guid()).ToList();
        }
    }
}