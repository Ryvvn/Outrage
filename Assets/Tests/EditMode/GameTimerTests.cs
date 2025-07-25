using NUnit.Framework;
using Core.Time;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Unit tests for GameTimer functionality.
    /// Tests timing calculations, state management, and edge cases.
    /// </summary>
    public class GameTimerTests
    {
        private GameTimer _timer;
        
        [SetUp]
        public void SetUp()
        {
            _timer = new GameTimer();
        }
        
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.AreEqual(0f, _timer.Duration, "Duration should be 0 initially");
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should be 0 initially");
            Assert.AreEqual(0f, _timer.RemainingTime, "Remaining time should be 0 initially");
            Assert.AreEqual(0f, _timer.Progress, "Progress should be 0 initially");
            Assert.IsFalse(_timer.IsRunning, "Should not be running initially");
            Assert.IsFalse(_timer.IsPaused, "Should not be paused initially");
            Assert.IsFalse(_timer.IsComplete, "Should not be complete initially");
        }
        
        [Test]
        public void Start_WithDuration_SetsUpCorrectly()
        {
            // Arrange
            float duration = 10f;
            
            // Act
            _timer.Start(duration);
            
            // Assert
            Assert.AreEqual(duration, _timer.Duration, "Duration should be set correctly");
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should be 0 at start");
            Assert.AreEqual(duration, _timer.RemainingTime, "Remaining time should equal duration at start");
            Assert.AreEqual(0f, _timer.Progress, "Progress should be 0 at start");
            Assert.IsTrue(_timer.IsRunning, "Should be running after start");
            Assert.IsFalse(_timer.IsPaused, "Should not be paused after start");
            Assert.IsFalse(_timer.IsComplete, "Should not be complete at start");
        }
        
        [Test]
        public void Update_AdvancesTime()
        {
            // Arrange
            _timer.Start(10f);
            float deltaTime = 2f;
            
            // Act
            _timer.Update(deltaTime);
            
            // Assert
            Assert.AreEqual(deltaTime, _timer.ElapsedTime, "Elapsed time should advance by delta");
            Assert.AreEqual(10f - deltaTime, _timer.RemainingTime, "Remaining time should decrease");
            Assert.AreEqual(deltaTime / 10f, _timer.Progress, "Progress should be calculated correctly");
            Assert.IsTrue(_timer.IsRunning, "Should still be running");
            Assert.IsFalse(_timer.IsComplete, "Should not be complete yet");
        }
        
        [Test]
        public void Update_CompletesTimer()
        {
            // Arrange
            _timer.Start(5f);
            
            // Act
            _timer.Update(6f); // More than duration
            
            // Assert
            Assert.AreEqual(5f, _timer.ElapsedTime, "Elapsed time should be clamped to duration");
            Assert.AreEqual(0f, _timer.RemainingTime, "Remaining time should be 0");
            Assert.AreEqual(1f, _timer.Progress, "Progress should be 1 (complete)");
            Assert.IsFalse(_timer.IsRunning, "Should not be running when complete");
            Assert.IsTrue(_timer.IsComplete, "Should be complete");
        }
        
        [Test]
        public void Pause_StopsTimeAdvancement()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(2f);
            
            // Act
            _timer.Pause();
            _timer.Update(3f); // This should not advance time
            
            // Assert
            Assert.AreEqual(2f, _timer.ElapsedTime, "Elapsed time should not advance when paused");
            Assert.IsTrue(_timer.IsRunning, "Should still be running (but paused)");
            Assert.IsTrue(_timer.IsPaused, "Should be paused");
        }
        
        [Test]
        public void Resume_ContinuesTimeAdvancement()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(2f);
            _timer.Pause();
            _timer.Update(3f); // Should not advance
            
            // Act
            _timer.Resume();
            _timer.Update(1f);
            
            // Assert
            Assert.AreEqual(3f, _timer.ElapsedTime, "Elapsed time should advance after resume");
            Assert.IsTrue(_timer.IsRunning, "Should be running");
            Assert.IsFalse(_timer.IsPaused, "Should not be paused");
        }
        
        [Test]
        public void Stop_ResetsTimer()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(5f);
            
            // Act
            _timer.Stop();
            
            // Assert
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should be reset");
            Assert.AreEqual(10f, _timer.RemainingTime, "Remaining time should be reset to duration");
            Assert.AreEqual(0f, _timer.Progress, "Progress should be reset");
            Assert.IsFalse(_timer.IsRunning, "Should not be running");
            Assert.IsFalse(_timer.IsPaused, "Should not be paused");
            Assert.IsFalse(_timer.IsComplete, "Should not be complete");
        }
        
        [Test]
        public void Reset_ResetsToInitialState()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(5f);
            
            // Act
            _timer.Reset();
            
            // Assert
            Assert.AreEqual(0f, _timer.Duration, "Duration should be reset");
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should be reset");
            Assert.AreEqual(0f, _timer.RemainingTime, "Remaining time should be reset");
            Assert.AreEqual(0f, _timer.Progress, "Progress should be reset");
            Assert.IsFalse(_timer.IsRunning, "Should not be running");
            Assert.IsFalse(_timer.IsPaused, "Should not be paused");
            Assert.IsFalse(_timer.IsComplete, "Should not be complete");
        }
        
        [Test]
        public void AddTime_IncreasesRemainingTime()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(3f);
            
            // Act
            _timer.AddTime(5f);
            
            // Assert
            Assert.AreEqual(15f, _timer.Duration, "Duration should be increased");
            Assert.AreEqual(3f, _timer.ElapsedTime, "Elapsed time should remain the same");
            Assert.AreEqual(12f, _timer.RemainingTime, "Remaining time should be increased");
            Assert.AreEqual(3f / 15f, _timer.Progress, "Progress should be recalculated");
        }
        
        [Test]
        public void SetRemainingTime_UpdatesCorrectly()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(3f);
            
            // Act
            _timer.SetRemainingTime(8f);
            
            // Assert
            Assert.AreEqual(11f, _timer.Duration, "Duration should be adjusted");
            Assert.AreEqual(3f, _timer.ElapsedTime, "Elapsed time should remain the same");
            Assert.AreEqual(8f, _timer.RemainingTime, "Remaining time should be set correctly");
            Assert.AreEqual(3f / 11f, _timer.Progress, "Progress should be recalculated");
        }
        
        [Test]
        public void ZeroDuration_HandledCorrectly()
        {
            // Act
            _timer.Start(0f);
            
            // Assert
            Assert.AreEqual(0f, _timer.Duration, "Duration should be 0");
            Assert.AreEqual(1f, _timer.Progress, "Progress should be 1 for zero duration");
            Assert.IsTrue(_timer.IsComplete, "Should be complete immediately");
            Assert.IsFalse(_timer.IsRunning, "Should not be running");
        }
        
        [Test]
        public void NegativeDuration_HandledCorrectly()
        {
            // Act
            _timer.Start(-5f);
            
            // Assert
            Assert.AreEqual(0f, _timer.Duration, "Negative duration should be clamped to 0");
            Assert.IsTrue(_timer.IsComplete, "Should be complete immediately");
            Assert.IsFalse(_timer.IsRunning, "Should not be running");
        }
        
        [Test]
        public void NegativeDeltaTime_HandledCorrectly()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(5f);
            
            // Act
            _timer.Update(-2f);
            
            // Assert
            Assert.AreEqual(5f, _timer.ElapsedTime, "Elapsed time should not decrease");
        }
        
        [Test]
        public void UpdateWhenNotRunning_DoesNothing()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Stop();
            
            // Act
            _timer.Update(5f);
            
            // Assert
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should not change when not running");
        }
        
        [Test]
        public void MultipleStartCalls_ResetsTimer()
        {
            // Arrange
            _timer.Start(10f);
            _timer.Update(5f);
            
            // Act
            _timer.Start(8f);
            
            // Assert
            Assert.AreEqual(8f, _timer.Duration, "Duration should be updated");
            Assert.AreEqual(0f, _timer.ElapsedTime, "Elapsed time should be reset");
            Assert.AreEqual(8f, _timer.RemainingTime, "Remaining time should be reset");
            Assert.AreEqual(0f, _timer.Progress, "Progress should be reset");
            Assert.IsTrue(_timer.IsRunning, "Should be running");
        }
    }
}