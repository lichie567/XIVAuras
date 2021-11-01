using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XIVAuras.Helpers
{
    public static class Singletons
    {
        private static readonly Dictionary<Type, Func<object>> TypeInitializers;

        private static ConcurrentDictionary<Type, object> ActiveInstances = new ConcurrentDictionary<Type, object>();

        static Singletons()
        {
            Singletons.TypeInitializers = new Dictionary<Type, Func<object>>()
            {
                { typeof(SpellHelpers), () => new SpellHelpers() }
            };
        }

        public static T Get<T>()
        {
            return (T)Singletons.ActiveInstances.GetOrAdd(typeof(T), (objectType) =>
            {
                object newInstance;
                if (Singletons.TypeInitializers.TryGetValue(objectType, out Func<object>? initializer))
                {
                    newInstance = initializer();
                }
                else
                {
                    throw new Exception($"No initializer found for Type '{objectType.FullName}'.");
                }

                if (newInstance is null || newInstance is not T)
                {
                    throw new Exception($"Received invalid result from initializer for type '{objectType.FullName}'");
                }

                return newInstance;
            });
        }

        public static void Register(object newSingleton)
        {
            if (!Singletons.ActiveInstances.TryAdd(newSingleton.GetType(), newSingleton))
            {
                throw new Exception($"Failed to register new singleton for type {newSingleton.GetType()}");
            }
        }

        public static void DisposeAll()
        {
            foreach (object singleton in ActiveInstances.Values)
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            ActiveInstances = new ConcurrentDictionary<Type, object>();
        }
    }
}