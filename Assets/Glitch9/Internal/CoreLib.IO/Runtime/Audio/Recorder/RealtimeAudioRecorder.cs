using System;
using Cysharp.Threading.Tasks;
using Glitch9.IO.Files;
using UnityEngine;

namespace Glitch9.CoreLib.IO.Audio
{
    public class RealtimeAudioRecorder : AudioRecorderBase
    {
        public enum State
        {
            NotInitialized,
            Idle,
            Speaking,
            Stopped,
        }

        public static class MaxValues
        {
            public const int DetectionIntervalMs = 10000; // 10 seconds
            public const int SilenceDurationMs = 300000; // 5 minutes
            public const float SilenceThreshold = 0.1f;
        }

        public static class MinValues
        {
            public const int DetectionIntervalMs = 50; // 0.05 seconds
            public const int SilenceDurationMs = 1000; // 1 second
            public const float SilenceThreshold = 0.005f;
        }

        #region Private Fields for Lazy Loading
        private float _silenceThreshold = 0.01f;
        private int _detectionIntervalMs = 100;
        private int _silenceDurationMs = 5000;
        private State? _state;

        #endregion Private Fields for Lazy Loading


        public float SilenceThreshold
        {
            get => _silenceThreshold;
            set
            {
                if (value < MinValues.SilenceThreshold || value > MaxValues.SilenceThreshold)
                {
                    _logger.Warning("Silence threshold must be between " + MinValues.SilenceThreshold + " and " + MaxValues.SilenceThreshold);
                    return;
                }
                _silenceThreshold = value;
            }
        }

        public int DetectionIntervalMs
        {
            get => _detectionIntervalMs;
            set
            {
                if (value < MinValues.DetectionIntervalMs || value > MaxValues.DetectionIntervalMs)
                {
                    _logger.Warning("Sample duration must be between " + MinValues.DetectionIntervalMs + " and " + MaxValues.DetectionIntervalMs);
                    return;
                }
                _detectionIntervalMs = value;
            }
        }

        public int SilenceDurationMs
        {
            get => _silenceDurationMs;
            set
            {
                if (value < MinValues.SilenceDurationMs || value > MaxValues.SilenceDurationMs)
                {
                    _logger.Warning("Silence duration must be between " + MinValues.SilenceDurationMs + " and " + MaxValues.SilenceDurationMs);
                    return;
                }
                _silenceDurationMs = value;
            }
        }

        public bool SilenceDetectionEnabled { get; set; } = true;


#pragma warning disable IDE1006

        public State state
        {
            get
            {
                _state ??= State.NotInitialized;
                return _state.Value;
            }
            private set
            {
                _state = value;
                onStateChanged?.Invoke(_state.Value);
            }
        }

        private bool canRecord => state == State.Speaking || state == State.Idle;
        public event Action<State> onStateChanged;
        public event Action<float> onAudioLevelChanged;

#pragma warning restore IDE1006

        private readonly Action<float[]> _onAudioDataAvailable; // Callback for handling streaming data
        private readonly Action _onSpeakingEnded; // Callback for when speaking ends  
        private int _lastSamplePosition = 0;
        private DateTime _lastInputTime;


        public RealtimeAudioRecorder(
            SampleRate sampleRate = SampleRate.Hz16000,
            int recordingLength = 30,
            string microphoneDeviceName = null,
            int detectionIntervalMs = 500,
            int silenceDurationMs = 2500,
            float silenceThreshold = 0.01f,
            bool silenceDetectionEnabled = true,
            Action<float[]> onAudioDataAvailable = null,
            Action onSpeakingEnded = null,
            ILogger logger = null,
            bool debugMode = false) : base(sampleRate, recordingLength, microphoneDeviceName, logger, debugMode)
        {
            if (detectionIntervalMs < MinValues.DetectionIntervalMs || detectionIntervalMs > MaxValues.DetectionIntervalMs)
            {
                throw new ArgumentOutOfRangeException(nameof(detectionIntervalMs), $"Sample duration must be between {MinValues.DetectionIntervalMs} and {MaxValues.DetectionIntervalMs} ms.");
            }

            if (silenceDurationMs < MinValues.SilenceDurationMs || silenceDurationMs > MaxValues.SilenceDurationMs)
            {
                throw new ArgumentOutOfRangeException(nameof(silenceDurationMs), $"Silence duration must be between {MinValues.SilenceDurationMs} and {MaxValues.SilenceDurationMs} ms.");
            }

            if (silenceThreshold < MinValues.SilenceThreshold || silenceThreshold > MaxValues.SilenceThreshold)
            {
                throw new ArgumentOutOfRangeException(nameof(silenceThreshold), $"Silence threshold must be between {MinValues.SilenceThreshold} and {MaxValues.SilenceThreshold}.");
            }

            _detectionIntervalMs = detectionIntervalMs;
            _silenceDurationMs = silenceDurationMs;
            _silenceThreshold = silenceThreshold;
            SilenceDetectionEnabled = silenceDetectionEnabled;
            _onAudioDataAvailable = onAudioDataAvailable;
            _onSpeakingEnded = onSpeakingEnded;

            _lastInputTime = DateTime.Now;
        }

        protected override void OnAudioClipReceived(AudioClip clip)
        {
            base.OnAudioClipReceived(clip);
            if (clip == null) return;

            _lastInputTime = DateTime.Now;
            state = State.Idle;

            // Begin the streaming process
            _lastSamplePosition = 0;

            bool analyzeAudioLevel = SilenceDetectionEnabled || onAudioLevelChanged != null;
            bool monitorSilence = SilenceDetectionEnabled || _onAudioDataAvailable != null || _onSpeakingEnded != null;

            if (analyzeAudioLevel) StreamAudioData().Forget();
            if (monitorSilence) MonitorSilence().Forget();
        }

        // Version 2
        public async UniTaskVoid StreamAudioData()
        {
            while (canRecord)
            {
                await UniTask.Delay(DetectionIntervalMs); // 지정된 시간 대기

                // 새로운 오디오 샘플이 있는지 확인
                int currentSamplePosition = Microphone.GetPosition(MicrophoneDeviceName);
                if (currentSamplePosition > _lastSamplePosition)
                {
                    int sampleLength = currentSamplePosition - _lastSamplePosition;
                    float[] audioData = new float[sampleLength];

                    RecordingClip.GetData(audioData, _lastSamplePosition);


                    // 오디오 입력 확인 후, 무음이 아닐 때만 데이터 전송
                    if (HasAudioInput(audioData))
                    {
                        _lastInputTime = DateTime.Now; // 마지막 입력 시간 갱신
                        state = State.Speaking;
                        _onAudioDataAvailable?.Invoke(audioData); // 오디오 데이터 전송

                        if (onAudioLevelChanged != null)
                        {
                            // 오디오 레벨 계산 후 이벤트 호출
                            float audioLevel = CalculateAudioLevel(audioData);
                            onAudioLevelChanged?.Invoke(audioLevel);
                        }

                        //_logger.Info("Audio input detected, streaming audio data.");
                    }
                    else
                    {
                        onAudioLevelChanged?.Invoke(0);
                        //_logger.Info("No significant audio input detected, skipping audio frame.");
                    }

                    _lastSamplePosition = currentSamplePosition;
                }
            }
        }

        private bool HasAudioInput(float[] audioData)
        {
            foreach (float sample in audioData)
            {
                if (Math.Abs(sample) > SilenceThreshold)
                {
                    return true; // There is input
                }
            }
            return false; // No input detected
        }

        // Version 2
        public async UniTaskVoid MonitorSilence()
        {
            while (canRecord)
            {
                await UniTask.Delay(DetectionIntervalMs);

                // 침묵 시간이 지나면 녹음을 멈추고 Idle 상태로 변경
                if ((DateTime.Now - _lastInputTime).TotalMilliseconds > SilenceDurationMs)
                {
                    if (_state == State.Speaking)
                    {
                        if (_debugMode) _logger.Info("Silence detected, switching to Idle state.");
                        state = State.Idle; // Idle 상태로 변경
                        _onSpeakingEnded?.Invoke(); // Speaking 종료 이벤트 호출
                    }
                }
            }

            // Idle 상태에서도 계속 소리를 감지하여 다시 녹음 시작
            while (state == State.Idle)
            {
                await UniTask.Delay(DetectionIntervalMs);

                // 소리가 다시 감지되면 녹음 재개
                int currentSamplePosition = Microphone.GetPosition(null);
                if (currentSamplePosition > _lastSamplePosition)
                {
                    int sampleLength = currentSamplePosition - _lastSamplePosition;
                    float[] audioData = new float[sampleLength];
                    RecordingClip.GetData(audioData, _lastSamplePosition);

                    if (HasAudioInput(audioData))
                    {
                        if (_debugMode) _logger.Info("Audio input detected, resuming recording.");
                        _lastInputTime = DateTime.Now;
                        state = State.Speaking; // 다시 Speaking 상태로 변경
                        _onAudioDataAvailable?.Invoke(audioData);
                        _lastSamplePosition = currentSamplePosition; // 샘플 포지션 갱신
                    }
                }
            }
        }

        public AudioClip StopRecording(State state = State.Stopped)
        {
            if (!canRecord)
            {
                _logger.Warning(Messages.kRecordingNotStarted);
                return null;
            }

            // Stop the recording
            Microphone.End(null);
            this.state = state;
            if (_debugMode) _logger.Info(Messages.kRecordingStopped);

            if (RecordingClip != null)
            {
                RecordingClip.TrimSilence();
                return RecordingClip;
            }

            _logger.Warning("No audio data recorded. Please try again.");
            return null;
        }

        public void ResumeRecording()
        {
            if (canRecord)
            {
                _logger.Warning(Messages.kRecordingAlreadyStarted);
                return;
            }

            if (_debugMode) _logger.Info("Resuming recording.");
            StartRecording();
        }

        private float CalculateAudioLevel(float[] audioData)
        {
            float sum = 0f;

            // 모든 오디오 샘플의 절대값을 합산
            foreach (float sample in audioData)
            {
                sum += Mathf.Abs(sample);
            }

            // 평균값을 구하고, 샘플 수로 나누어 정규화된 오디오 레벨 계산
            float average = sum / audioData.Length;

            // 오디오 레벨을 0에서 1 사이의 값으로 정규화
            return Mathf.Clamp01(average / SilenceThreshold);
        }
    }
}
