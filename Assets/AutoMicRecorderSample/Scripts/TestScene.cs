using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;


namespace hayatsukikazumi.amr
{
	/// <summary>
	/// Test scene of Automatic microphone recorder.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>1.0.2</version>
	/// <since>2017/11/04</since>
	public class TestScene : MonoBehaviour
	{
		public GameObject microphone;
		public Text statusText;
		public Button calibButton;
		public Button resetMicButton;

		private AutoMicRecorder amr;
		private AudioSource playSource;

		private PreRecordedSpeechToTextService speech2Text;
		private string textResult = "";
		private bool initialAutoCalib;

		// Use this for initialization
		void Start()
		{
			playSource = gameObject.AddComponent<AudioSource>();
			playSource.playOnAwake = false;

			//Get AutoMicRecorder.
			amr = microphone.GetComponent<AutoMicRecorder>();
			initialAutoCalib = amr.autoCalibration;

			//SpeeceToText service.
			speech2Text = gameObject.GetComponent<PreRecordedSpeechToTextService>();
			if (speech2Text != null) {
				speech2Text.OnTextResult += OnTextResult;
				speech2Text.OnError += OnTransrateError;
			}

			//add action to buttons.
			calibButton.onClick.AddListener(DoExecCalibration);
			resetMicButton.onClick.AddListener(DoExecResetMicrophone);

			//Add event handlers.
			amr.RecordingStopped.AddListener((AutoMicRecorder a, bool b) => {
				Replay();
			});

			amr.RecordingTimeout.AddListener((AutoMicRecorder a, bool b) => {
				Replay();
			});

			amr.CalibrationEnd.AddListener((AutoMicRecorder a, bool b) => {
				if (!b) amr.autoRecording = true;
			});

			amr.MicrophoneStarted.AddListener((AutoMicRecorder a, bool b) => {
				if (b) a.CalibrateThreshold();
			});
		}
	
		// Update is called once per frame
		void Update()
		{
			statusText.text = string.Format("status:\t{0}\nlevel:\t{1:0.0000}\ns.level:\t{2:0.0000}\n"
				+ "threshold:\t{3:0.0000}\nautoRecording:\t{4}\nreplaying:\t{5}\nresult:\t{6}",
				amr.status, amr.soundLevel, amr.smoothedSoundLevel,
				amr.threshold, amr.autoRecording, playSource.isPlaying, textResult);

			if (!amr.autoRecording && !playSource.isPlaying) {
				amr.autoRecording = true;
			}
		}

		void Replay() {
			AudioClip clip = amr.GetRecordedClip();
			amr.autoRecording = false;
			Debug.Log("Play. time=" + clip.length);

			if (speech2Text != null && speech2Text.enabled) {
				speech2Text.Transrate(clip);
			}

			float[] data = new float[clip.samples];
			clip.GetData(data, 0);
			Array.Reverse(data);
			clip.SetData(data, 0);

			playSource.clip = clip;
			playSource.Play();
		}

		void DoExecCalibration() {
			amr.CalibrateThreshold();
		}

		void DoExecResetMicrophone() {
			amr.StartMicrophone();
		}

		void OnTextResult(SpeechToTextResult result) {
			TextAlternative[] a = result.TextAlternatives;
			textResult = "";
			for (int i = 0; i < a.Length; i++) {
				if (i > 0) textResult += "/";
				textResult += result.TextAlternatives[i].Text;
				Debug.Log(i + ":" + result.TextAlternatives[i].Text);
			}
		}

		void OnTransrateError(string message) {
			textResult = "ERROR:" + message;
			Debug.Log("ERROR:" + message);
		}
	}
}