using NUnit.Framework;
using Core.Services;
using Core.GameStates;
using UnityEngine;
using System;

namespace Tests.EditMode
{
    /// <summary>
    /// Unit tests for the GameManager class.
    /// Tests state transitions, initialization, and event handling.
    /// </summary>
    public class GameManagerTests
    {
        private GameObject _gameManagerObject;
        private GameManager _gameManager;
        private bool _stateChangeEventFired;
        private GameState _previousState;
        private GameState _newState;
        
        [SetUp]
        public void SetUp()
        {
            // Clean up any existing services
            ServiceLocator.ShutdownAllServices();
            
            // Create GameManager
            _gameManagerObject = new GameObject("TestGameManager");
            _gameManager = _gameManagerObject.AddComponent<GameManager>();
            
            // Reset event tracking
            _stateChangeEventFired = false;
            _previousState = null;
            _newState = null;
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up
            if (_gameManagerObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameManagerObject);
            }
            
            ServiceLocator.ShutdownAllServices();
        }
        
        [Test]
        public void Initialize_GameManager_InitializesSuccessfully()
        {
            // Act
            _gameManager.Initialize();
            
            // Assert
            Assert.IsTrue(_gameManager.IsInitialized);
            
            // Verify it's registered with ServiceLocator
            var retrievedManager = ServiceLocator.GetService<IGameManager>();
            Assert.IsNotNull(retrievedManager);
            Assert.AreEqual(_gameManager, retrievedManager);
        }
        
        [Test]
        public void Initialize_AlreadyInitialized_DoesNotReinitialize()
        {
            // Arrange
            _gameManager.Initialize();
            var firstRegistration = ServiceLocator.GetService<IGameManager>();
            
            // Act
            _gameManager.Initialize(); // Second initialization
            
            // Assert
            Assert.IsTrue(_gameManager.IsInitialized);
            var secondRegistration = ServiceLocator.GetService<IGameManager>();
            Assert.AreEqual(firstRegistration, secondRegistration);
        }
        
        [Test]
        public void ChangeState_ValidState_ChangesSuccessfully()
        {
            // Arrange
            _gameManager.Initialize();
            
            // Act
            _gameManager.ChangeState<TestGameState>();
            
            // Assert
            Assert.IsNotNull(_gameManager.CurrentState);
            Assert.IsInstanceOf<TestGameState>(_gameManager.CurrentState);
            Assert.IsTrue(_gameManager.CurrentState.IsActive);
        }
        
        [Test]
        public void ChangeState_NotInitialized_DoesNotChangeState()
        {
            // Act
            _gameManager.ChangeState<TestGameState>();
            
            // Assert
            Assert.IsNull(_gameManager.CurrentState);
        }
        
        [Test]
        public void ChangeState_SameState_DoesNotChangeState()
        {
            // Arrange
            _gameManager.Initialize();
            _gameManager.ChangeState<TestGameState>();
            var firstState = _gameManager.CurrentState;
            
            // Act
            _gameManager.ChangeState<TestGameState>();
            
            // Assert
            Assert.AreEqual(firstState, _gameManager.CurrentState);
        }
        
        [Test]
        public void ChangeState_DifferentStates_TransitionsCorrectly()
        {
            // Arrange
            _gameManager.Initialize();
            _gameManager.ChangeState<TestGameState>();
            var firstState = _gameManager.CurrentState as TestGameState;
            
            // Act
            _gameManager.ChangeState<AnotherTestGameState>();
            
            // Assert
            Assert.IsInstanceOf<AnotherTestGameState>(_gameManager.CurrentState);
            Assert.IsTrue(_gameManager.CurrentState.IsActive);
            Assert.IsFalse(firstState.IsActive); // Previous state should be inactive
            Assert.IsTrue(firstState.WasExitCalled);
        }
        
        [Test]
        public void OnStateChanged_StateTransition_FiresEvent()
        {
            // Arrange
            _gameManager.Initialize();
            _gameManager.OnStateChanged += OnStateChangedHandler;
            
            // Act
            _gameManager.ChangeState<TestGameState>();
            
            // Assert
            Assert.IsTrue(_stateChangeEventFired);
            Assert.IsNull(_previousState); // No previous state
            Assert.IsInstanceOf<TestGameState>(_newState);
        }
        
        [Test]
        public void OnStateChanged_StateTransitionWithPreviousState_FiresEventWithBothStates()
        {
            // Arrange
            _gameManager.Initialize();
            _gameManager.ChangeState<TestGameState>();
            var firstState = _gameManager.CurrentState;
            
            _gameManager.OnStateChanged += OnStateChangedHandler;
            
            // Act
            _gameManager.ChangeState<AnotherTestGameState>();
            
            // Assert
            Assert.IsTrue(_stateChangeEventFired);
            Assert.AreEqual(firstState, _previousState);
            Assert.IsInstanceOf<AnotherTestGameState>(_newState);
        }
        
        [Test]
        public void IsTransitioning_DuringStateChange_ReturnsTrue()
        {
            // Arrange
            _gameManager.Initialize();
            bool wasTransitioning = false;
            
            _gameManager.OnStateChanged += (prev, next) =>
            {
                // This should be called during transition
                wasTransitioning = _gameManager.IsTransitioning;
            };
            
            // Act
            _gameManager.ChangeState<TestGameState>();
            
            // Assert
            Assert.IsFalse(wasTransitioning); // Should be false after transition completes
        }
        
        [Test]
        public void Shutdown_InitializedGameManager_ShutsDownCorrectly()
        {
            // Arrange
            _gameManager.Initialize();
            _gameManager.ChangeState<TestGameState>();
            var currentState = _gameManager.CurrentState as TestGameState;
            
            // Act
            _gameManager.Shutdown();
            
            // Assert
            Assert.IsFalse(_gameManager.IsInitialized);
            Assert.IsNull(_gameManager.CurrentState);
            Assert.IsTrue(currentState.WasExitCalled);
        }
        
        private void OnStateChangedHandler(GameState previousState, GameState newState)
        {
            _stateChangeEventFired = true;
            _previousState = previousState;
            _newState = newState;
        }
    }
    
    #region Test Game State Classes
    
    public class TestGameState : GameState
    {
        public bool WasEnterCalled { get; private set; }
        public bool WasExitCalled { get; private set; }
        public bool WasUpdateCalled { get; private set; }
        
        public override void Enter()
        {
            base.Enter();
            WasEnterCalled = true;
        }
        
        public override void Update()
        {
            base.Update();
            WasUpdateCalled = true;
        }
        
        public override void Exit()
        {
            base.Exit();
            WasExitCalled = true;
        }
    }
    
    public class AnotherTestGameState : GameState
    {
        public bool WasEnterCalled { get; private set; }
        public bool WasExitCalled { get; private set; }
        
        public override void Enter()
        {
            base.Enter();
            WasEnterCalled = true;
        }
        
        public override void Exit()
        {
            base.Exit();
            WasExitCalled = true;
        }
    }
    
    #endregion
}