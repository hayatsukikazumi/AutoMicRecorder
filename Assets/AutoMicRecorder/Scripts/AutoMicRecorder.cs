using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// Recorder status.
	/// </summary>
	public enum RecorderStatus { READY, RECORDING, CALIBRATION, NOT_READY, SENSOR_SURPRESSING }

	/// <summary>
	/// Event of AutoMicRecorder, extends UnityEvent.
	/// </summary>
	/// <version>2.0.0</version>
	[Serializable]
	public class AutoMicRecorderEvent : UnityEvent<AutoMicRecorder, bool>
	{
	}

	/// <summary>
	/// Automatic microphone recorder.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.2.0</version>
	/// <since>2017/11/04</since>
	public class AutoMicRecorder : MonoBehaviour
	{

		//microphone

		/// <summary>
		/// Executing StartMicrophone() on Start() of MonoBehavior.
		/// </summary>
		public bool playOnStart = true;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AutoMicRecorder"/> auto recording.
		/// </summary>
		/// <value><c>true</c> if recording automatically; otherwise, <c>false</c>.</value>
		public bool autoRecording = true;

		private const float THRESHOLD_MIN = 0.0001f;
		private const float THRESHOLD_MAX = 1;

		/// <summary>
		/// Gets or sets the threshold.
		/// </summary>
		/// <value>The threshold. Set the value greater than 0 and less than 1.</value>
		[RangeAttribute(THRESHOLD_MIN,THRESHOLD_MAX)]
		public float threshold = 0.02f;

		/// <summary>
		/// When autoRecording property and this is true, adjusting the threshold automatically.
		/// </summary>
		public bool autoCalibration = true;

		/// <summary>
		/// Gets or sets the sensivity of auto calibration and CalibrateThreshold method.
		/// </summary>
		/// <value>Set positive value to be more sensitive, and set negative value to be more insensitive.</value>
		[RangeAttribute(-1f, 1f)]
		public float sensivity = 0f;

		/// <summary>
		/// Microphone device name for recording.
		/// If you pass a null or empty string for the device name then the default microphone will be used. 
		/// </summary>
		public string micDeviceName;

		/// <summary>
		/// Microphone sampling rate(Hz).
		/// </summary>
		[SerializeField]
		protected int micSampleRate = 16000;

		/// <summary>
		/// Maximum recording time(sec).
		/// </summary>
		[Header("Time Settings (sec.)")]
		[SerializeField]
		protected float maxRecordTime = 15;

		public float beforeGap = 0.5f;
		public float sensorOn = 0.125f;
		public float sensorOnKeep = 0.5f;
		public float sensorOff = 1.25f;
		public float sensorOffKeep = 0.5f;
		public float afterGap = 0.25f;

		/// <summary>
		/// Maximam wait time for StartMicrophone().
		/// </summary>
		public float maxInitTime = 10;

		/// <summary>
		/// Occurs when recording started.
		/// </summary>
		public AutoMicRecorderEvent RecordingStarted;

		/// <summary>
		/// Occurs when recording stopped.
		/// </summary>
		public AutoMicRecorderEvent RecordingStopped;

		/// <summary>
		/// Occurs when recording timeout.
		/// </summary>
		public AutoMicRecorderEvent RecordingTimeout;

		/// <summary>
		/// Occurs when calibration end.
		/// </summary>
		public AutoMicRecorderEvent CalibrationEnd;

		/// <summary>
		/// Occurs when StartMicrophone() end.
		/// </summary>
		public AutoMicRecorderEvent MicrophoneStarted;

		/// <summary>
		/// Occurs when StartMicrophone() failed.
		/// </summary>
		public AutoMicRecorderEvent MicrophoneStartFailed;

		//level sensor
		private float[] sensorBuf = null;
		private float sensorTime = 0;
		private int lastSamplePos = 0;
		private bool lastIsAutoRecording = false;

		private bool inRecording = false;
		private int startPos = 0;
		private float[] recordedData;

		//others
		private AudioSource microphone;
		private enum CalibStatus { None, InAuto, InManual };

		private CalibStatus inCalibThreshold = CalibStatus.None;

		/// <summary>
		/// Gets the status.
		/// </summary>
		/// <value>The status.</value>
		public RecorderStatus status {
			get {
				if (sensorBuf == null) return RecorderStatus.NOT_READY;
				else if (inRecording) return RecorderStatus.RECORDING;
				else if (inCalibThreshold == CalibStatus.InManual) return RecorderStatus.CALIBRATION;
				else if (sensorTime < 0) return RecorderStatus.SENSOR_SURPRESSING;
				else return RecorderStatus.READY;
			}
		}

		/// <summary>
		/// Gets the recording time.
		/// </summary>
		/// <value>The recording time(sec).</value>
		public float recordingTime {
			get;
			private set;
		}

		/// <summary>
		/// Gets the sound level.
		/// </summary>
		/// <value>The sound level. returns value between 0 and 1.</value>
		public float soundLevel {
			get;
			private set;
		}

		/// <summary>
		/// Gets the sound level for automatic sensor, absorbed attack.
		/// </summary>
		/// <value>The sound level. returns value between 0 and 1.</value>
		public float smoothedSoundLevel {
			get;
			private set;
		}

		/// <summary>
		/// Starts the recording.
		/// </summary>
		/// <returns><c>true</c>, if recording was started, <c>false</c> otherwise.</returns>
		public bool StartRecording() {
			return DoStartRecording(false);
		}

		private bool DoStartRecording(bool isAutomatic) {
			if (inRecording || sensorBuf == null) return false;
			inRecording = true;

			startPos = microphone.timeSamples;
			if (isAutomatic) startPos -= (int)(micSampleRate * (sensorOn + beforeGap));
			if (startPos < 0) startPos += microphone.clip.samples;

			recordingTime = isAutomatic ? sensorOn + beforeGap : 0;
			sensorTime = Math.Min(0, -sensorOnKeep);

			//Debug.Log("recording started. @" + audio.time);
			if (RecordingStarted != null) RecordingStarted.Invoke(this, isAutomatic);

			return true;
		}

		/// <summary>
		/// Stops the recording.
		/// </summary>
		/// <returns><c>true</c>, if recording was stoped, <c>false</c> otherwise.</returns>
		public bool StopRecording() {
			return DoStopRecording(false);
		}

		private bool DoStopRecording(bool isAutomatic) {

			if (!inRecording) return false;
			inRecording = false;

			int sampleLen = microphone.timeSamples - startPos;
			int clipSamples = microphone.clip.samples;
			if (isAutomatic && recordingTime < maxRecordTime) sampleLen -= (int)(micSampleRate * (sensorOff - afterGap));
			if (sampleLen <= 0) sampleLen += clipSamples;
			sampleLen = Mathf.Min(sampleLen, (int)(micSampleRate * maxRecordTime));

			recordedData = new float[sampleLen];
			if (startPos + sampleLen <= clipSamples) {
				microphone.clip.GetData(recordedData, startPos);
			} else {
				float[] rd = new float[clipSamples - startPos];
				microphone.clip.GetData(rd, startPos);
				Array.Copy(rd, 0, recordedData, 0, rd.Length);
				rd = new float[sampleLen - rd.Length];
				microphone.clip.GetData(rd, 0);
				Array.Copy(rd, 0, recordedData, clipSamples - startPos, rd.Length);
			}

			//invalidate sensor.
			sensorTime = Math.Min(0, -sensorOffKeep);

			if (recordingTime < maxRecordTime) {
				//Debug.Log("recording stopped. @" + audio.time);
				if (RecordingStopped != null) RecordingStopped.Invoke(this, isAutomatic);
			} else {
				//Debug.Log("recording timeout. @" + audio.time);
				if (RecordingTimeout != null) RecordingTimeout.Invoke(this, true);
			}

			return true;
		}

		/// <summary>
		/// Gets the recorded clip.
		/// </summary>
		/// <returns>The recorded clip.</returns>
		public AudioClip GetRecordedClip()
		{
			AudioClip clip = AudioClip.Create("recordedClip", recordedData.Length, microphone.clip.channels, microphone.clip.frequency, false);
			clip.SetData(recordedData, 0);
			return clip;
		}

		/// <summary>
		/// Calibrates the threshold. When recoeding, this method no works.
		/// </summary>
		public void CalibrateThreshold()
		{
			if (status == RecorderStatus.NOT_READY || status == RecorderStatus.RECORDING) return;

			StopAllCoroutines();
			StartCoroutine(DoCalibrateThreshold(false, THRESHOLD_MIN, THRESHOLD_MAX));
		}

		/// <summary>
		/// Resets the microphone. Recorded data is discarded if during recording.
		/// </summary>
		/// <param name="newSampleRate">New smapling rate.</param>
		public void StartMicrophone(int newSampleRate) {
			StopAllCoroutines();
			StartCoroutine(DoResetMicrophone(newSampleRate, false));
		}

		/// <summary>
		/// Resets the microphone in current sampling rate. Recorded data is discarded if during recording.
		/// </summary>
		public void StartMicrophone() {
			StartMicrophone(micSampleRate);
		}

		public void StopMicrophone() {
			//invalidate sensor.
			sensorBuf = null;
			inRecording = false;

			if (microphone != null) {
				microphone.Stop();
				if (microphone.clip != null) {
					GameObject.Destroy(microphone.clip);
				}
			}
		}

		private IEnumerator DoResetMicrophone(int newSampleRate, bool isAutomatic) {
			//invalidate sensor.
			StopMicrophone();

			int fmin = -1;
			int fmax = -1;
			for (int i = 0; i < Microphone.devices.Length; i++) {
				string device = Microphone.devices[i];
				Microphone.GetDeviceCaps(device, out fmin, out fmax);
				Debug.Log(i + ":Name: " + device + " min:" + fmin + " max:" + fmax);
			}

			if (Microphone.devices.Length == 0) {
				yield return new WaitForSeconds(maxInitTime);
				if (MicrophoneStartFailed != null) MicrophoneStartFailed.Invoke(this, isAutomatic);
				yield break;
			}

			//initialize audio.
			microphone = GetComponent<AudioSource>();
			microphone.loop = true;
			microphone.mute = false;

			int micRecordTime = Mathf.CeilToInt(beforeGap + sensorOn + maxRecordTime + sensorOff + afterGap + 1);
			Debug.Log("micRecordTime:" + micRecordTime);

			microphone.clip = Microphone.Start(micDeviceName, true, micRecordTime, newSampleRate);
			yield return null;

			float wtime = 0;
			while (Microphone.GetPosition(micDeviceName) <= 0) {
				wtime += Time.deltaTime;
				if (wtime > this.maxInitTime) {
					if (MicrophoneStartFailed != null) MicrophoneStartFailed.Invoke(this, isAutomatic);
					yield break;
				}

				//wait for maicrophone is ready.
				yield return null;
			}
			microphone.Play();
			yield return null;

			micSampleRate = newSampleRate;

			//reset sensor.
			sensorTime = 0;
			lastSamplePos = 0;
			sensorBuf = new float[micSampleRate / 100];	// samples 1/100 sec.

			if (MicrophoneStarted != null) MicrophoneStarted.Invoke(this, isAutomatic);
		}

		// Use this for initialization
		void Awake()
		{
			recordingTime = 0;
			soundLevel = 0;
			smoothedSoundLevel = 0;
		}

		// Use this for initialization
		void Start()
		{
			if (playOnStart) {
				StopAllCoroutines();
				StartCoroutine(DoResetMicrophone(micSampleRate, true));
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (sensorBuf == null) {
				soundLevel = 0;
				smoothedSoundLevel = 0;
				return;
			}

			//reset smoothedSoundLevel.
			if (!lastIsAutoRecording && autoRecording) {
				smoothedSoundLevel = 0;
			}
			lastIsAutoRecording = autoRecording;

			//get the max volume.
			microphone.clip.GetData(sensorBuf, lastSamplePos);
			int samplePos = microphone.timeSamples;
			if (lastSamplePos != samplePos) {
				lastSamplePos = microphone.timeSamples;
				soundLevel = Mathf.Max(Mathf.Max(sensorBuf), -Mathf.Min(sensorBuf), 0);
				smoothedSoundLevel = smoothedSoundLevel * 0.75f + soundLevel * 0.25f;
			}

			//timeout check.
			if (inRecording) {
				recordingTime += Time.unscaledDeltaTime;

				if (recordingTime >= maxRecordTime) {
					DoStopRecording(true);
					return;
				}
			}

			//auto start/stop.
			if (autoRecording && inCalibThreshold != CalibStatus.InManual) {
				if (sensorTime < 0) {
					sensorTime += Time.unscaledDeltaTime;

				} else if (!inRecording && IsOverThreshold(false)) {
					sensorTime += Time.unscaledDeltaTime;
					if (sensorTime >= sensorOn) {
						DoStartRecording(true);
					}

				} else if (inRecording && !IsOverThreshold(true)) {
					sensorTime += Time.unscaledDeltaTime;
					if (sensorTime >= sensorOff) {
						DoStopRecording(true);
					}

				} else {
					sensorTime = 0;
				}
			} else {
				sensorTime = 0;
			}

			//auto calibrarion.
			if (autoCalibration && !inRecording && inCalibThreshold == CalibStatus.None && sensorTime == 0) {
				StartCoroutine(DoCalibrateThreshold(true, threshold * 0.9f, threshold * 1.1f));
			}
		}

		private bool IsOverThreshold(bool isInRecording) {
			return smoothedSoundLevel >= threshold;
		}

		private IEnumerator DoCalibrateThreshold(bool isAuto, float clampMin, float clampMax) {
			
			inCalibThreshold = isAuto ? CalibStatus.InAuto : CalibStatus.InManual;

			//get the average of middle 12 samples from 24 samples.
			float[] samples = new float[24];
			for (int i = 0; i < 24; i++) {
				samples[i] = soundLevel;

				if (inRecording) {
					inCalibThreshold = CalibStatus.None;
					yield break;
				}

				yield return null;
			}

			Array.Sort(samples);
			float ave = 0;
			for (int i = 6; i < 18; i++) {
				ave += samples[i];
			}
			ave /= 12;

			if (inRecording) {
				inCalibThreshold = CalibStatus.None;
				yield break;
			}

			threshold = Mathf.Clamp(Mathf.Pow(ave * 0.96f + 0.0004f, 0.625f + sensivity * 0.3125f),
				clampMin, clampMax);
			//Debug.Log("New threshold:" + threshold);

			inCalibThreshold = CalibStatus.None;
			if (CalibrationEnd != null) CalibrationEnd.Invoke(this, isAuto);
		}
	}
}
