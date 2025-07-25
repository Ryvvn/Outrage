using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Services
{
    /// <summary>
    /// Central service registry and access point for all game services.
    /// Implements singleton pattern with thread-safe service access.
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        private static readonly object _lock = new object();
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        
        /// <summary>
        /// Gets the singleton instance of the ServiceLocator.
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            var go = new GameObject("ServiceLocator");
                            _instance = go.AddComponent<ServiceLocator>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The service instance, or null if not found.</returns>
        public static T GetService<T>() where T : class, IService
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (Instance._services.TryGetValue(type, out var service))
                {
                    return service as T;
                }
                return null;
            }
        }
        
        /// <summary>
        /// Registers a service with the locator.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public static void RegisterService<T>(T service) where T : class, IService
        {
            if (service == null)
            {
                Debug.LogError($"Cannot register null service of type {typeof(T).Name}");
                return;
            }
            
            lock (_lock)
            {
                var type = typeof(T);
                if (Instance._services.ContainsKey(type))
                {
                    Debug.LogWarning($"Service of type {type.Name} is already registered. Replacing existing service.");
                }
                
                Instance._services[type] = service;
                Debug.Log($"Registered service: {type.Name}");
            }
        }
        
        /// <summary>
        /// Unregisters a service from the locator.
        /// </summary>
        /// <typeparam name="T">The type of service to unregister.</typeparam>
        public static void UnregisterService<T>() where T : class, IService
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (Instance._services.TryGetValue(type, out var service))
                {
                    service.Shutdown();
                    Instance._services.Remove(type);
                    Debug.Log($"Unregistered service: {type.Name}");
                }
            }
        }
        
        /// <summary>
        /// Shuts down all registered services and clears the registry.
        /// </summary>
        public static void ShutdownAllServices()
        {
            lock (_lock)
            {
                foreach (var service in Instance._services.Values)
                {
                    try
                    {
                        service.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error shutting down service {service.GetType().Name}: {ex.Message}");
                    }
                }
                Instance._services.Clear();
                Debug.Log("All services shut down");
            }
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                ShutdownAllServices();
            }
        }
        
        private void OnApplicationQuit()
        {
            ShutdownAllServices();
        }
    }
}
