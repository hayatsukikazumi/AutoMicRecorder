using System;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// JSON request for Google Cloud Speech-to-Text API(v1).
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.0.0</version>
	/// <since>2018/05/20</since>
	[Serializable]
	public class GoogleJSONRequest
	{
		public GoogleJSONRecognitionConfig config;
		public GoogleJSONRecognitionAudio audio;
	}

	[Serializable]
	public class GoogleJSONRecognitionConfig
	{
		public string encoding;
		public int sampleRateHertz;
		public string languageCode;
		public int maxAlternatives;
	}

	[Serializable]
	public class GoogleJSONRecognitionAudio
	{
		public string content;
	}
}
