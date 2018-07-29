using System.Collections.Generic;

namespace BeeInstant.NetSDK.Utils
{
    internal static class IDictionaryExtensions
    {
        public static void AddOrUpdate<K, V>(this IDictionary<K, V> dict, K key, V value)
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

        public static V GetOrAdd<K, V>(this IDictionary<K, V> dict, K key, V value)
        {
            if(dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                dict.Add(key, value);
                return value;
            }
        }
    }
}