using NUnit.Framework;
using Core.Services;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EditMode
{
    /// <summary>
    /// Unit tests for the ServiceLocator class.
    /// Tests service registration, retrieval, and thread safety.
    /// </summary>
    public class ServiceLocatorTests
    {
        private TestService _testService;
        
        [SetUp]
        public void SetUp()
        {
            // Clean up any existing services before each test
            ServiceLocator.ShutdownAllServices();
            _testService = new TestService();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            ServiceLocator.ShutdownAllServices();
            _testService = null;
        }
        
        [Test]
        public void RegisterService_ValidService_RegistersSuccessfully()
        {
            // Arrange & Act
            ServiceLocator.RegisterService<ITestService>(_testService);
            
            // Assert
            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.IsNotNull(retrievedService);
            Assert.AreEqual(_testService, retrievedService);
        }
        
        [Test]
        public void GetService_ServiceNotRegistered_ReturnsNull()
        {
            // Act
            var service = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.IsNull(service);
        }
        
        [Test]
        public void RegisterService_NullService_LogsError()
        {
            // Arrange
            TestService nullService = null;
            
            // Act & Assert
            // This should not throw an exception, but should log an error
            Assert.DoesNotThrow(() => ServiceLocator.RegisterService<ITestService>(nullService));
            
            // Verify service was not registered
            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.IsNull(retrievedService);
        }
        
        [Test]
        public void RegisterService_ReplaceExistingService_ReplacesSuccessfully()
        {
            // Arrange
            var firstService = new TestService();
            var secondService = new TestService();
            
            // Act
            ServiceLocator.RegisterService<ITestService>(firstService);
            ServiceLocator.RegisterService<ITestService>(secondService);
            
            // Assert
            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.AreEqual(secondService, retrievedService);
            Assert.AreNotEqual(firstService, retrievedService);
        }
        
        [Test]
        public void UnregisterService_RegisteredService_UnregistersSuccessfully()
        {
            // Arrange
            ServiceLocator.RegisterService<ITestService>(_testService);
            
            // Act
            ServiceLocator.UnregisterService<ITestService>();
            
            // Assert
            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.IsNull(retrievedService);
            Assert.IsTrue(_testService.WasShutdownCalled);
        }
        
        [Test]
        public void UnregisterService_ServiceNotRegistered_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => ServiceLocator.UnregisterService<ITestService>());
        }
        
        [Test]
        public void ShutdownAllServices_MultipleServices_ShutsDownAll()
        {
            // Arrange
            var service1 = new TestService();
            var service2 = new AnotherTestService();
            
            ServiceLocator.RegisterService<ITestService>(service1);
            ServiceLocator.RegisterService<IAnotherTestService>(service2);
            
            // Act
            ServiceLocator.ShutdownAllServices();
            
            // Assert
            Assert.IsTrue(service1.WasShutdownCalled);
            Assert.IsTrue(service2.WasShutdownCalled);
            
            Assert.IsNull(ServiceLocator.GetService<ITestService>());
            Assert.IsNull(ServiceLocator.GetService<IAnotherTestService>());
        }
        
        [Test]
        public async Task ServiceLocator_ConcurrentAccess_IsThreadSafe()
        {
            // Arrange
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var tasks = new Task[threadCount];
            var services = new TestService[threadCount];
            
            for (int i = 0; i < threadCount; i++)
            {
                services[i] = new TestService();
            }
            
            // Act - Create multiple threads that register and retrieve services
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        // Register service
                        ServiceLocator.RegisterService<ITestService>(services[threadIndex]);
                        
                        // Retrieve service
                        var retrievedService = ServiceLocator.GetService<ITestService>();
                        
                        // Small delay to increase chance of race conditions
                        Thread.Sleep(1);
                    }
                });
            }
            
            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            
            // Assert - Should not throw any exceptions and should have a service registered
            var finalService = ServiceLocator.GetService<ITestService>();
            Assert.IsNotNull(finalService);
        }
    }
    
    #region Test Service Classes
    
    public interface ITestService : IService
    {
        bool WasShutdownCalled { get; }
    }
    
    public interface IAnotherTestService : IService
    {
        bool WasShutdownCalled { get; }
    }
    
    public class TestService : ITestService
    {
        public bool IsInitialized { get; private set; }
        public bool WasShutdownCalled { get; private set; }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            WasShutdownCalled = true;
            IsInitialized = false;
        }
    }
    
    public class AnotherTestService : IAnotherTestService
    {
        public bool IsInitialized { get; private set; }
        public bool WasShutdownCalled { get; private set; }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            WasShutdownCalled = true;
            IsInitialized = false;
        }
    }
    
    #endregion
}