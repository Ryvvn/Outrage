using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Core.Services;
using Core.GameStates;
using Core.Bootstrap;

namespace Tests.PlayMode
{
    /// <summary>
    /// Integration tests for core systems working together.
    /// Tests the complete initialization flow and state transitions.
    /// </summary>
    public class CoreSystemsIntegrationTests
    {
        private GameObject _bootstrapObject;
        private GameBootstrap _bootstrap;
        
        [SetUp]
        public void SetUp()
        {
            // Clean up any existing services
            ServiceLocator.ShutdownAllServices();
            
            // Create bootstrap object
            _bootstrapObject = new GameObject("TestBootstrap");
            _bootstrap = _bootstrapObject.AddComponent<GameBootstrap>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up
            if (_bootstrapObject != null)
            {
                Object.DestroyImmediate(_bootstrapObject);
            }
            
            ServiceLocator.ShutdownAllServices();
        }
        
        [UnityTest]
        public IEnumerator GameBootstrap_InitializesServicesAndStatesCorrectly()
        {
            // Act - Start the bootstrap process
            _bootstrap.InitializeGame();
            
            // Wait a frame for initialization
            yield return null;
            
            // Assert - GameManager should be registered and initialized
            var gameManager = ServiceLocator.GetService<IGameManager>();
            Assert.IsNotNull(gameManager, "GameManager should be registered in ServiceLocator");
            Assert.IsTrue(gameManager.IsInitialized, "GameManager should be initialized");
            
            // Assert - Should start in InitializationState
            Assert.IsNotNull(gameManager.CurrentState, "GameManager should have a current state");
            Assert.AreEqual("InitializationState", gameManager.CurrentState.GetStateName(), 
                "Should start in InitializationState");
        }
        
        [UnityTest]
        public IEnumerator GameManager_StateTransition_WorksCorrectly()
        {
            // Arrange
            _bootstrap.InitializeGame();
            yield return null;
            
            var gameManager = ServiceLocator.GetService<IGameManager>();
            bool stateChangeEventFired = false;
            GameState previousState = null;
            GameState newState = null;
            
            // Subscribe to state change event
            gameManager.OnStateChanged += (prev, next) =>
            {
                stateChangeEventFired = true;
                previousState = prev;
                newState = next;
            };
            
            // Act - Change to MenuState
            gameManager.ChangeState<MenuState>();
            yield return null;
            
            // Assert
            Assert.IsTrue(stateChangeEventFired, "State change event should fire");
            Assert.IsNotNull(newState, "New state should not be null");
            Assert.AreEqual("MenuState", newState.GetStateName(), "Should transition to MenuState");
            Assert.AreEqual("MenuState", gameManager.CurrentState.GetStateName(), 
                "GameManager current state should be MenuState");
        }
        
        [UnityTest]
        public IEnumerator ServiceLocator_RegisterAndRetrieveServices_WorksInPlayMode()
        {
            // Arrange
            var testService = new TestPlayModeService();
            
            // Act
            ServiceLocator.RegisterService<ITestPlayModeService>(testService);
            yield return null;
            
            var retrievedService = ServiceLocator.GetService<ITestPlayModeService>();
            
            // Assert
            Assert.IsNotNull(retrievedService, "Should be able to retrieve registered service");
            Assert.AreSame(testService, retrievedService, "Retrieved service should be the same instance");
            Assert.IsTrue(retrievedService.IsInitialized, "Service should be initialized");
        }
        
        [UnityTest]
        public IEnumerator GameBootstrap_Shutdown_CleansUpProperly()
        {
            // Arrange
            _bootstrap.InitializeGame();
            yield return null;
            
            var gameManager = ServiceLocator.GetService<IGameManager>();
            Assert.IsNotNull(gameManager, "GameManager should be registered");
            
            // Act - Shutdown
            _bootstrap.ShutdownGame();
            yield return null;
            
            // Assert - Services should be cleaned up
            Assert.IsFalse(gameManager.IsInitialized, "GameManager should be shutdown");
            
            // Try to get service after shutdown - should return null or throw
            var serviceAfterShutdown = ServiceLocator.GetService<IGameManager>();
            // Note: Depending on implementation, this might be null or the same instance but shutdown
            if (serviceAfterShutdown != null)
            {
                Assert.IsFalse(serviceAfterShutdown.IsInitialized, "Service should be shutdown");
            }
        }
        
        [UnityTest]
        public IEnumerator MultipleStateTransitions_WorkCorrectly()
        {
            // Arrange
            _bootstrap.InitializeGame();
            yield return null;
            
            var gameManager = ServiceLocator.GetService<IGameManager>();
            
            // Act & Assert - Multiple state transitions
            gameManager.ChangeState<MenuState>();
            yield return null;
            Assert.AreEqual("MenuState", gameManager.CurrentState.GetStateName());
            
            gameManager.ChangeState<GameplayState>();
            yield return null;
            Assert.AreEqual("GameplayState", gameManager.CurrentState.GetStateName());
            
            gameManager.ChangeState<InitializationState>();
            yield return null;
            Assert.AreEqual("InitializationState", gameManager.CurrentState.GetStateName());
        }
    }
    
    #region Test Services
    
    public interface ITestPlayModeService : IService
    {
        string TestData { get; }
    }
    
    public class TestPlayModeService : ITestPlayModeService
    {
        public bool IsInitialized { get; private set; }
        public string TestData { get; private set; }
        
        public void Initialize()
        {
            IsInitialized = true;
            TestData = "Test Service Initialized";
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
            TestData = null;
        }
    }
    
    #endregion
}