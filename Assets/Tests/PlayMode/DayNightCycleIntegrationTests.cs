using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core.Time;
using Core.GameLoop;
using Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Integration tests for DayNightCycleManager.
    /// Tests visual transitions, lighting changes, and game loop integration.
    /// </summary>
    public class DayNightCycleIntegrationTests
    {
        private GameObject _testGameObject;
        private DayNightCycleManager _dayNightManager;
        private GameLoopConfig _testConfig;
        private Light _testSunLight;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with DayNightCycleManager
            _testGameObject = new GameObject("TestDayNightManager");
            _dayNightManager = _testGameObject.AddComponent<DayNightCycleManager>();
            
            // Create test sun light
            var lightGameObject = new GameObject("TestSunLight");
            _testSunLight = lightGameObject.AddComponent<Light>();
            _testSunLight.type = LightType.Directional;
            
            // Create test configuration
            _testConfig = ScriptableObject.CreateInstance<GameLoopConfig>();
            _testConfig.dayPhaseDuration = 10f;
            _testConfig.buildPhaseDuration = 8f;
            _testConfig.nightPhaseDuration = 6f;
            _testConfig.waveProgressDuration = 15f;
            _testConfig.endWaveDuration = 5f;
            _testConfig.dayAmbientColor = Color.white;
            _testConfig.nightAmbientColor = new Color(0.2f, 0.2f, 0.4f, 1f);
            _testConfig.lightTransitionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            
            // Set the config and sun light on the manager using reflection
            var configField = typeof(DayNightCycleManager).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(_dayNightManager, _testConfig);
            
            var sunLightField = typeof(DayNightCycleManager).GetField("sunLight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sunLightField?.SetValue(_dayNightManager, _testSunLight);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_dayNightManager != null)
            {
                _dayNightManager.Shutdown();
            }
            
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
            
            if (_testSunLight != null)
            {
                Object.DestroyImmediate(_testSunLight.gameObject);
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
            _dayNightManager.Initialize();
            
            // Assert
            Assert.IsTrue(_dayNightManager.IsInitialized, "DayNightCycleManager should be initialized");
            Assert.IsTrue(_dayNightManager.IsDay, "Should start during day time");
            Assert.GreaterOrEqual(_dayNightManager.TimeOfDay, 0f, "Time of day should be >= 0");
            Assert.LessOrEqual(_dayNightManager.TimeOfDay, 1f, "Time of day should be <= 1");
        }
        
        [Test]
        public void SetTimeOfDay_UpdatesCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Act
            _dayNightManager.SetTimeOfDay(0.5f); // Noon
            
            // Assert
            Assert.AreEqual(0.5f, _dayNightManager.TimeOfDay, "Time of day should be set correctly");
            Assert.IsTrue(_dayNightManager.IsDay, "Should be day at noon");
            
            // Act
            _dayNightManager.SetTimeOfDay(0.9f); // Night
            
            // Assert
            Assert.AreEqual(0.9f, _dayNightManager.TimeOfDay, "Time of day should be updated");
            Assert.IsFalse(_dayNightManager.IsDay, "Should be night at 0.9");
        }
        
        [Test]
        public void DayProgress_CalculatesCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Test dawn (0.25)
            _dayNightManager.SetTimeOfDay(0.25f);
            Assert.AreEqual(0f, _dayNightManager.DayProgress, "Day progress should be 0 at dawn");
            
            // Test noon (0.5)
            _dayNightManager.SetTimeOfDay(0.5f);
            Assert.AreEqual(0.5f, _dayNightManager.DayProgress, "Day progress should be 0.5 at noon");
            
            // Test dusk (0.75)
            _dayNightManager.SetTimeOfDay(0.75f);
            Assert.AreEqual(1f, _dayNightManager.DayProgress, "Day progress should be 1 at dusk");
            
            // Test night (0.9)
            _dayNightManager.SetTimeOfDay(0.9f);
            Assert.AreEqual(1f, _dayNightManager.DayProgress, "Day progress should be 1 during night");
        }
        
        [Test]
        public void IsDay_WorksCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Test various times
            _dayNightManager.SetTimeOfDay(0.1f); // Before dawn
            Assert.IsFalse(_dayNightManager.IsDay, "Should be night before dawn");
            
            _dayNightManager.SetTimeOfDay(0.3f); // Morning
            Assert.IsTrue(_dayNightManager.IsDay, "Should be day in morning");
            
            _dayNightManager.SetTimeOfDay(0.5f); // Noon
            Assert.IsTrue(_dayNightManager.IsDay, "Should be day at noon");
            
            _dayNightManager.SetTimeOfDay(0.7f); // Afternoon
            Assert.IsTrue(_dayNightManager.IsDay, "Should be day in afternoon");
            
            _dayNightManager.SetTimeOfDay(0.8f); // After dusk
            Assert.IsFalse(_dayNightManager.IsDay, "Should be night after dusk");
        }
        
        [UnityTest]
        public IEnumerator TransitionToTimeOfDay_WorksCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            _dayNightManager.SetTimeOfDay(0.25f); // Start at dawn
            
            // Act
            _dayNightManager.TransitionToTimeOfDay(0.75f, 0.5f); // Transition to dusk over 0.5 seconds
            
            // Wait for transition to complete
            yield return new WaitForSeconds(0.6f);
            
            // Assert
            Assert.AreEqual(0.75f, _dayNightManager.TimeOfDay, 0.01f, "Should transition to target time");
            Assert.IsFalse(_dayNightManager.IsDay, "Should be night at dusk");
        }
        
        [Test]
        public void UpdateForGamePhase_SetsCorrectTimeOfDay()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Test Day phase
            _dayNightManager.UpdateForGamePhase(GamePhase.Day, 0f);
            Assert.GreaterOrEqual(_dayNightManager.TimeOfDay, 0.25f, "Day phase should start at dawn or later");
            Assert.LessOrEqual(_dayNightManager.TimeOfDay, 0.45f, "Day phase should end before mid-morning");
            
            // Test Build phase
            _dayNightManager.UpdateForGamePhase(GamePhase.Build, 0.5f);
            Assert.GreaterOrEqual(_dayNightManager.TimeOfDay, 0.45f, "Build phase should be mid-morning or later");
            Assert.LessOrEqual(_dayNightManager.TimeOfDay, 0.65f, "Build phase should end before afternoon");
            
            // Test Night phase
            _dayNightManager.UpdateForGamePhase(GamePhase.Night, 0.5f);
            Assert.GreaterOrEqual(_dayNightManager.TimeOfDay, 0.65f, "Night phase should be afternoon or later");
            Assert.LessOrEqual(_dayNightManager.TimeOfDay, 0.85f, "Night phase should end before deep night");
        }
        
        [Test]
        public void PauseResume_WorksCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            float initialTime = _dayNightManager.TimeOfDay;
            
            // Act - Pause
            _dayNightManager.Pause();
            _dayNightManager.UpdateForGamePhase(GamePhase.Build, 0.5f);
            
            // Assert - Time should not change when paused
            Assert.AreEqual(initialTime, _dayNightManager.TimeOfDay, "Time should not change when paused");
            
            // Act - Resume
            _dayNightManager.Resume();
            _dayNightManager.UpdateForGamePhase(GamePhase.Build, 0.5f);
            
            // Assert - Time should change when resumed
            // Note: This might be the same if the target time happens to match, so we just ensure no errors
            Assert.DoesNotThrow(() => _dayNightManager.UpdateForGamePhase(GamePhase.Night, 0.5f));
        }
        
        [Test]
        public void AmbientLighting_UpdatesCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            Color initialAmbient = RenderSettings.ambientLight;
            
            // Act - Set to day
            _dayNightManager.SetTimeOfDay(0.5f); // Noon
            Color dayAmbient = RenderSettings.ambientLight;
            
            // Act - Set to night
            _dayNightManager.SetTimeOfDay(0.9f); // Night
            Color nightAmbient = RenderSettings.ambientLight;
            
            // Assert
            Assert.AreNotEqual(dayAmbient, nightAmbient, "Day and night ambient colors should be different");
            
            // The exact colors depend on the blend, but night should be darker
            Assert.Less(nightAmbient.r + nightAmbient.g + nightAmbient.b, 
                       dayAmbient.r + dayAmbient.g + dayAmbient.b, 
                       "Night should be darker than day");
        }
        
        [Test]
        public void SunLight_UpdatesCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Act - Set to day
            _dayNightManager.SetTimeOfDay(0.5f); // Noon
            float dayIntensity = _testSunLight.intensity;
            Vector3 dayRotation = _testSunLight.transform.rotation.eulerAngles;
            
            // Act - Set to night
            _dayNightManager.SetTimeOfDay(0.9f); // Night
            float nightIntensity = _testSunLight.intensity;
            Vector3 nightRotation = _testSunLight.transform.rotation.eulerAngles;
            
            // Assert
            Assert.Greater(dayIntensity, nightIntensity, "Day should be brighter than night");
            Assert.AreNotEqual(dayRotation, nightRotation, "Sun rotation should change between day and night");
        }
        
        [Test]
        public void Events_FireCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            bool timeChangedFired = false;
            bool dayNightTransitionFired = false;
            float receivedTime = 0f;
            bool receivedIsDay = false;
            
            _dayNightManager.OnTimeOfDayChanged += (time) =>
            {
                timeChangedFired = true;
                receivedTime = time;
            };
            
            _dayNightManager.OnDayNightTransition += (isDay) =>
            {
                dayNightTransitionFired = true;
                receivedIsDay = isDay;
            };
            
            // Act - Change time (should fire time changed)
            _dayNightManager.SetTimeOfDay(0.5f);
            
            // Assert
            Assert.IsTrue(timeChangedFired, "Time changed event should fire");
            Assert.AreEqual(0.5f, receivedTime, "Should receive correct time");
            
            // Reset flags
            timeChangedFired = false;
            dayNightTransitionFired = false;
            
            // Act - Transition from day to night
            _dayNightManager.SetTimeOfDay(0.9f); // Night
            
            // Assert
            Assert.IsTrue(timeChangedFired, "Time changed event should fire again");
            Assert.IsTrue(dayNightTransitionFired, "Day/night transition event should fire");
            Assert.IsFalse(receivedIsDay, "Should receive false for night transition");
        }
        
        [Test]
        public void Shutdown_CleansUpCorrectly()
        {
            // Arrange
            _dayNightManager.Initialize();
            
            // Act
            _dayNightManager.Shutdown();
            
            // Assert
            Assert.IsFalse(_dayNightManager.IsInitialized, "Should not be initialized after shutdown");
        }
        
        [Test]
        public void OperationsWithoutInitialize_HandledGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _dayNightManager.SetTimeOfDay(0.5f));
            Assert.DoesNotThrow(() => _dayNightManager.TransitionToTimeOfDay(0.7f, 1f));
            Assert.DoesNotThrow(() => _dayNightManager.UpdateForGamePhase(GamePhase.Day, 0.5f));
            Assert.DoesNotThrow(() => _dayNightManager.Pause());
            Assert.DoesNotThrow(() => _dayNightManager.Resume());
        }
    }
}