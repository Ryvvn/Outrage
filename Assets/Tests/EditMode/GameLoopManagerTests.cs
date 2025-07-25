using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core.GameLoop;
using Core.Time;
using Data;
using System.Collections;

namespace Tests.EditMode
{
    /// <summary>
    /// Unit tests for GameLoopManager functionality.
    /// Tests phase transitions, timing, and state management.
    /// </summary>
    public class GameLoopManagerTests
    {
        private GameObject _testGameObject;
        private GameLoopManager _gameLoopManager;
        private GameLoopConfig _testConfig;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with GameLoopManager
            _testGameObject = new GameObject("TestGameLoopManager");
            _gameLoopManager = _testGameObject.AddComponent<GameLoopManager>();
            
            // Create test configuration
            _testConfig = ScriptableObject.CreateInstance<GameLoopConfig>();
            _testConfig.dayPhaseDuration = 10f;
            _testConfig.buildPhaseDuration = 8f;
            _testConfig.nightPhaseDuration = 6f;
            _testConfig.waveProgressDuration = 15f;
            _testConfig.endWaveDuration = 5f;
            _testConfig.dayAmbientColor = Color.white;
            _testConfig.nightAmbientColor = Color.blue;
            _testConfig.lightTransitionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            
            // Set the config on the manager
            var configField = typeof(GameLoopManager).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(_gameLoopManager, _testConfig);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_gameLoopManager != null)
            {
                _gameLoopManager.Shutdown();
            }
            
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
            
            if (_testConfig != null)
            {
                Object.DestroyImmediate(_testConfig);
            }
        }
        
        [Test]
        public void Initialize_SetsUpManagerCorrectly()
        {
            // Act
            _gameLoopManager.Initialize();
            
            // Assert
            Assert.IsTrue(_gameLoopManager.IsInitialized, "GameLoopManager should be initialized");
            Assert.AreEqual(GamePhase.Day, _gameLoopManager.CurrentPhase, "Should start in Day phase");
            Assert.IsFalse(_gameLoopManager.IsRunning, "Should not be running initially");
            Assert.IsFalse(_gameLoopManager.IsPaused, "Should not be paused initially");
        }
        
        [Test]
        public void StartLoop_BeginsGameLoop()
        {
            // Arrange
            _gameLoopManager.Initialize();
            
            // Act
            _gameLoopManager.StartLoop();
            
            // Assert
            Assert.IsTrue(_gameLoopManager.IsRunning, "Game loop should be running");
            Assert.AreEqual(GamePhase.Day, _gameLoopManager.CurrentPhase, "Should start in Day phase");
            Assert.IsTrue(_gameLoopManager.TimeRemaining > 0, "Should have time remaining");
        }
        
        [Test]
        public void StopLoop_StopsGameLoop()
        {
            // Arrange
            _gameLoopManager.Initialize();
            _gameLoopManager.StartLoop();
            
            // Act
            _gameLoopManager.StopLoop();
            
            // Assert
            Assert.IsFalse(_gameLoopManager.IsRunning, "Game loop should not be running");
        }
        
        [Test]
        public void PauseResume_WorksCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            _gameLoopManager.StartLoop();
            
            // Act - Pause
            _gameLoopManager.Pause();
            
            // Assert - Paused
            Assert.IsTrue(_gameLoopManager.IsPaused, "Should be paused");
            Assert.IsTrue(_gameLoopManager.IsRunning, "Should still be running while paused");
            
            // Act - Resume
            _gameLoopManager.Resume();
            
            // Assert - Resumed
            Assert.IsFalse(_gameLoopManager.IsPaused, "Should not be paused");
            Assert.IsTrue(_gameLoopManager.IsRunning, "Should be running");
        }
        
        [Test]
        public void ForcePhaseTransition_ChangesPhaseCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            _gameLoopManager.StartLoop();
            
            // Act
            _gameLoopManager.ForcePhaseTransition(GamePhase.Build);
            
            // Assert
            Assert.AreEqual(GamePhase.Build, _gameLoopManager.CurrentPhase, "Should transition to Build phase");
            Assert.IsTrue(_gameLoopManager.TimeRemaining > 0, "Should have time remaining in new phase");
        }
        
        [Test]
        public void PhaseProgress_CalculatesCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            _gameLoopManager.StartLoop();
            
            // Act
            float initialProgress = _gameLoopManager.PhaseProgress;
            
            // Assert
            Assert.GreaterOrEqual(initialProgress, 0f, "Progress should be >= 0");
            Assert.LessOrEqual(initialProgress, 1f, "Progress should be <= 1");
        }
        
        [Test]
        public void PhaseTransitionEvents_FireCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            
            bool eventFired = false;
            GamePhase fromPhase = GamePhase.Day;
            GamePhase toPhase = GamePhase.Day;
            
            _gameLoopManager.OnPhaseChanged += (from, to) =>
            {
                eventFired = true;
                fromPhase = from;
                toPhase = to;
            };
            
            _gameLoopManager.StartLoop();
            
            // Act
            _gameLoopManager.ForcePhaseTransition(GamePhase.Build);
            
            // Assert
            Assert.IsTrue(eventFired, "Phase change event should fire");
            Assert.AreEqual(GamePhase.Day, fromPhase, "From phase should be Day");
            Assert.AreEqual(GamePhase.Build, toPhase, "To phase should be Build");
        }
        
        [Test]
        public void TimeUpdateEvents_FireCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            
            bool eventFired = false;
            float receivedTimeRemaining = 0f;
            float receivedProgress = 0f;
            
            _gameLoopManager.OnTimeUpdated += (timeRemaining, progress) =>
            {
                eventFired = true;
                receivedTimeRemaining = timeRemaining;
                receivedProgress = progress;
            };
            
            // Act
            _gameLoopManager.StartLoop();
            
            // Assert
            Assert.IsTrue(eventFired, "Time update event should fire");
            Assert.GreaterOrEqual(receivedTimeRemaining, 0f, "Time remaining should be >= 0");
            Assert.GreaterOrEqual(receivedProgress, 0f, "Progress should be >= 0");
            Assert.LessOrEqual(receivedProgress, 1f, "Progress should be <= 1");
        }
        
        [Test]
        public void Shutdown_CleansUpCorrectly()
        {
            // Arrange
            _gameLoopManager.Initialize();
            _gameLoopManager.StartLoop();
            
            // Act
            _gameLoopManager.Shutdown();
            
            // Assert
            Assert.IsFalse(_gameLoopManager.IsInitialized, "Should not be initialized after shutdown");
            Assert.IsFalse(_gameLoopManager.IsRunning, "Should not be running after shutdown");
        }
        
        [Test]
        public void InvalidPhaseTransition_HandledGracefully()
        {
            // Arrange
            _gameLoopManager.Initialize();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _gameLoopManager.ForcePhaseTransition((GamePhase)999));
        }
        
        [Test]
        public void MultipleInitialize_HandledGracefully()
        {
            // Arrange & Act
            _gameLoopManager.Initialize();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _gameLoopManager.Initialize());
            Assert.IsTrue(_gameLoopManager.IsInitialized, "Should remain initialized");
        }
        
        [Test]
        public void OperationsWithoutInitialize_HandledGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _gameLoopManager.StartLoop());
            Assert.DoesNotThrow(() => _gameLoopManager.StopLoop());
            Assert.DoesNotThrow(() => _gameLoopManager.Pause());
            Assert.DoesNotThrow(() => _gameLoopManager.Resume());
            Assert.DoesNotThrow(() => _gameLoopManager.ForcePhaseTransition(GamePhase.Build));
        }
    }
}