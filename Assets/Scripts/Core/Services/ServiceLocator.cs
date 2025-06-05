using System;
using System.Collections.Generic;

namespace ChaosCosmos.Core.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        public static void Register<T>(T serviceInstance) where T : IService
        {
            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }

            if (_services.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"Service of type {typeof(T).Name} already registered.");
            }

            _services[typeof(T)] = serviceInstance;
        }

        public static T Get<T>() where T : IService
        {
            if (_services.TryGetValue(typeof(T), out IService serviceInstance))
            {
                return (T)serviceInstance;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} not registered.");
        }

        public static void Unregister<T>() where T : IService
        {
            _services.Remove(typeof(T));
        }

        public static void UnregisterAll()
        {
            _services.Clear();
        }

        public static bool IsRegistered<T>() where T : IService
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}
