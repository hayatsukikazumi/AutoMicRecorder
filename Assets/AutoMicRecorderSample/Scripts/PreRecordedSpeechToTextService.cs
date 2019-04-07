using System;
using System.Collections;
using UnityEngine;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// SpeechToTextService for pre-recorded audio clip.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.0.0</version>
	/// <since>2017/10/15</since>
	public abstract class PreRecordedSpeechToTextService : MonoBehaviour {

		/// <summary>
		/// Action on text result received from service.
		/// </summary>
		public Action<SpeechToTextResult> OnTextResult;

		/// <summary>
		/// Action on error occurred from service.
		/// </summary>
		public Action<string> OnError;

		/// <summary>
		/// Transrate the specified audioClip.
		/// </summary>
		/// <param name="clip">audio clip.</param>
		public void Transrate(AudioClip clip) {
			StartCoroutine(TranslateRecordingToText(clip));
		}

		/// <summary>
		/// Translates speech to text by making a request to the speech-to-text API.
		/// </summary>
		protected abstract IEnumerator TranslateRecordingToText(AudioClip clip);
	}
}
