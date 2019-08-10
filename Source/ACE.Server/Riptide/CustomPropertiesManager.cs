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
        public static Property<bool> GetBool(string key)
        {
            return new Property<bool>(CachedBooleanSettings[key].Item, CachedBooleanSettings[key].Description);
        }

        public static Property<double> GetDouble(string key)
        {
            return new Property<double>(CachedDoubleSettings[key].Item, CachedDoubleSettings[key].Description);
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

        public static bool ModifyDouble(string key, double newVal)
        {
            if (!DefaultDoubleProperties.ContainsKey(key))
                return false;

            if (CachedDoubleSettings.ContainsKey(key))
                CachedDoubleSettings[key].Modify(newVal);
            else
                CachedDoubleSettings[key] = new ConfigurationEntry<double>(true, newVal, DefaultDoubleProperties[key].Description);
            return true;
        }

        private static readonly ReadOnlyDictionary<string, Property<bool>> DefaultBooleanProperties =
            DictOf(
                ("fix_point_blank_missiles", new Property<bool>(true, "Enable/disable the point-blank missile fix."))
            );
        private static readonly ReadOnlyDictionary<string, Property<double>> DefaultDoubleProperties =
            DictOf(
                ("fix_point_blank_missiles_factor", new Property<double>(1.0, "Set range factor for point-blank missile fix."))
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

            //double
            foreach (var item in DefaultDoubleProperties)
                ModifyDouble(item.Key, item.Value.Item);

            Console.WriteLine($"Got it");
        }
    }
}
