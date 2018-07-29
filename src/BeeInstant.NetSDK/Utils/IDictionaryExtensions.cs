using System.Collections.Generic;

namespace BeeInstant.NetSDK.Utils
{
    public static class IDictionaryExtensions
    {
        public static void AddOrUpdate<K, V>(this SortedDictionary<K, V> dict, K key, V value)
        {
            if(dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}