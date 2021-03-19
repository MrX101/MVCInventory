using UnityEditor;
using UnityEngine;

namespace Game.Timers
{
    [System.Serializable]
    public class Cooldown
    {
        private float _whenIStartedCounting = _HasNotStarted;
        private float _cooldownDuration;
        
        private bool _updateCooldownDuration = false;
        private float _cacheCooldownValue = 0f;

        private const float _HasNotStarted = -111;
        
        ///Might be Inaccurate if the current timer hasn't finished and it hasn't reset yet.
        public float CooldownDuration
        {
            get => _cooldownDuration;
        }

        public Cooldown(float duration)
        {
            this._cooldownDuration = duration;
        }

        public float GetTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return (float)EditorApplication.timeSinceStartup -_whenIStartedCounting;
            }
#endif
            return Time.time -_whenIStartedCounting;
        }
        
        public void ResetTimer()
        {
            this._whenIStartedCounting = _HasNotStarted;
        }

        public bool HasStarted()
        {
            return _whenIStartedCounting != _HasNotStarted;
        }
        
        ///Returns True if hasn't started
        public bool HasFinished(bool resetTimerIfFinished = false)
        {
            if (!HasStarted()) { return true; }
            var time = GetTime();
            if (time >= _cooldownDuration)
            {
                if (resetTimerIfFinished) { ResetTimer(); }
                return true;
            }
            return false;
        }

        public void StartCountDown()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _whenIStartedCounting = (float)EditorApplication.timeSinceStartup;
                if (_updateCooldownDuration)
                {
                    _cooldownDuration = _cacheCooldownValue;
                    _cacheCooldownValue = 0f;
                    _updateCooldownDuration = false;
                }
                return;
            }
#endif
            if (_updateCooldownDuration)
            {
                _cooldownDuration = _cacheCooldownValue;
                _cacheCooldownValue = 0f;
                _updateCooldownDuration = false;
            }
            _whenIStartedCounting = Time.time;
        }

        public void SetDuration(float newDuration, bool waitForCountdown = true)
        {
            if (HasStarted() && waitForCountdown)
            {
                _updateCooldownDuration = true;
                _cacheCooldownValue = newDuration;
            }
            else
            {
                _cooldownDuration = newDuration;
            }
        }
       

        public static void StartTimer(Cooldown cooldown)
        {
            cooldown.ResetTimer();
            cooldown.StartCountDown();
        }
    }
}

