using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HTTP;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// SpeechToTextService with Google Cloud Speech API for pre-recorded audio clip.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.1.0</version>
	/// <since>2017/11/04</since>
	public class GooglePreRecordedSpeechToTextService : PreRecordedSpeechToTextService {

		/// <summary>
		/// Store for APIKey property
		/// </summary>
		[SerializeField]
		string m_APIKey;

		/// <summary>
		/// Key used to authenticate requests to the Google API
		/// </summary>
		public string APIKey { set { m_APIKey = value; } }

		/// <summary>
		/// The language of the supplied audio as a BCP-47 language tag.
		/// </summary>
		[SerializeField]
		string languageCode;

		/// <summary>
		/// Maximum number of recognition hypotheses to be returned.
		/// </summary>
		[SerializeField]
		int maxAlternatives = 1;

		private const string API_URL = "https://speech.googleapis.com/v1/speech:recognize";
		private const string API_PARAM_APIKEY = "key";

		void Start() {
		}

		/// <summary>
		/// Translates speech to text by making a request to the speech-to-text API.
		/// </summary>
		protected override IEnumerator TranslateRecordingToText(AudioClip clip) {

			var request = new Request("POST", API_URL +
				"?" + API_PARAM_APIKEY + "=" + m_APIKey);
			request.headers.Add("Content-Type", "application/json");

			// Construct JSON request body.
			var reqJSON = new GoogleJSONRequest();
			var reqConf = new GoogleJSONRecognitionConfig();
			var reqAudio = new GoogleJSONRecognitionAudio();

			reqConf.encoding = "LINEAR16";
			reqConf.sampleRateHertz = clip.frequency;
			reqConf.languageCode = languageCode;
			reqConf.maxAlternatives = maxAlternatives;

			reqAudio.content = Convert.ToBase64String(ConvertToLinear16(clip));
			reqJSON.config = reqConf;
			reqJSON.audio = reqAudio;

			request.Text = JsonUtility.ToJson(reqJSON);
			request.Send();
			Debug.Log("GooglePreRecordedSpeechToTextService: Sent request");

			while (!request.isDone)
			{
				yield return null;
			}

			// Response.
			string responseText = request.response.Text;
			if (responseText == null) 
			{
				responseText = "";
			}

			// Parse error response.
			string errorMessage = null;
			try {
				GoogleJSONResponseError ers =  JsonUtility.FromJson<GoogleJSONResponseError>(responseText);
				if (ers != null && ers.error != null && ers.error.code != 0) {
					errorMessage = "(" + ers.error.code + ")"
						+ (string.IsNullOrEmpty(ers.error.message) ? "unknown error" :  ers.error.message);
				} else if (request.response.status >= 400) {
					errorMessage = "(" + request.response.status + ")Network or Http error";
				}
			} catch (ArgumentException ae) {
				errorMessage = "(" + request.response.status + ")" + ae.Message;
			}
	
			if (errorMessage != null) {
				Debug.Log("GooglePreRecordedSpeechToTextService: Error returned." + errorMessage);
				if (OnError != null) {
					OnError(errorMessage);
				}
				yield break;
			}

			// Parse response.
			GoogleJSONResponse resp = JsonUtility.FromJson<GoogleJSONResponse>(responseText);
			SpeechToTextResult textResult = new SpeechToTextResult();
			if (resp != null && resp.results != null && resp.results.Length > 0)
			{
				var resp1 = resp.results[0];
				if (resp1 != null && resp1.alternatives.Length > 0)
				{
					textResult.TextAlternatives = resp1.alternatives;
				}
			}

			if (textResult.TextAlternatives == null || textResult.TextAlternatives.Length == 0)
			{
				textResult.TextAlternatives = new TextAlternative[1];
				TextAlternative textAlt = new GoogleJSONRecognitionAlternative();
				textAlt.Text = "";
				textResult.TextAlternatives[0] = textAlt;
			}

			Debug.Log("GooglePreRecordedSpeechToTextService: Response returned.");
			if (OnTextResult != null)
			{
				OnTextResult(textResult);
			}
		}

		static byte[] ConvertToLinear16(AudioClip clip) {

			float[] samples = new float[clip.samples];
			clip.GetData(samples, 0);

			byte[] ret = new byte[clip.samples * 2];	//16Bit

			int idx = 0;
			for (int i = 0; i < clip.samples; i++) {
				int d = Math.Min((int)(samples[i] * 32768), 32767);
				ret[idx++] = (byte)(d & 255);
				d >>= 8;
				ret[idx++] = (byte)(d & 255);
			}

			return ret;
		}
	}

}