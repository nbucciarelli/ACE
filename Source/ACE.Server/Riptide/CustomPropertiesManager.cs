using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ACE.Server.Managers;

namespace ACE.Server.Riptide
{
    public static class CustomPropertiesManager
    {
        public static Property<bool> GetBool(string key, bool fallback = false, bool cacheFallback = true)
        {
            return new Property<bool>(CachedBooleanSettings[key].Item, CachedBooleanSettings[key].Description);
        }

        public static bool ModifyBool(string key, bool newVal)
        {
            if (!DefaultBooleanProperties.ContainsKey(key))
                return false;

            if (CachedBooleanSettings.ContainsKey(key))
                CachedBooleanSettings[key].Modify(newVal);
            else
                CachedBooleanSettings[key] = new ConfigurationEntry<bool>(true, newVal, DefaultBooleanProperties[key].Description);
            return true;
        }

        private static readonly ReadOnlyDictionary<string, Property<bool>> DefaultBooleanProperties =
            DictOf(
                ("fix_point_blank_missiles", new Property<bool>(true, "Enable/disable the point-blank missile fix."))
            );

        public static string ListProperties()
        {
            string props = "Boolean properties:\n";
            foreach (var item in DefaultBooleanProperties)
                props += string.Format("\t{0}: {1} (current is {2}, default is {3})\n", item.Key, item.Value.Description, GetBool(item.Key).Item, item.Value.Item);

            return props;
        }

        private static ReadOnlyDictionary<A, V> DictOf<A, V>(params (A, V)[] pairs)
        {
            return new ReadOnlyDictionary<A, V>(pairs.ToDictionary
            (
                tup => tup.Item1,
                tup => tup.Item2
            ));
        }

        private static readonly ConcurrentDictionary<string, ConfigurationEntry<bool>> CachedBooleanSettings = new ConcurrentDictionary<string, ConfigurationEntry<bool>>();
        private static readonly ConcurrentDictionary<string, ConfigurationEntry<long>> CachedLongSettings = new ConcurrentDictionary<string, ConfigurationEntry<long>>();
        private static readonly ConcurrentDictionary<string, ConfigurationEntry<double>> CachedDoubleSettings = new ConcurrentDictionary<string, ConfigurationEntry<double>>();
        private static readonly ConcurrentDictionary<string, ConfigurationEntry<string>> CachedStringSettings = new ConcurrentDictionary<string, ConfigurationEntry<string>>();

        public static void Initialize()
        {
            // Place any default properties to load in here

            //bool
            foreach (var item in DefaultBooleanProperties)
                ModifyBool(item.Key, item.Value.Item);
            Console.WriteLine($"Got it");
        }
    }
}
