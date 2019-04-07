using System;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// JSON response of Google Cloud Speech-to-Text API(v1).
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.0.0</version>
	/// <since>2018/05/20</since>
	[Serializable]
	public class GoogleJSONResponse
	{
		public GoogleJSONRecognitionResult[] results;
	}

	[Serializable]
	public class GoogleJSONRecognitionResult
	{
		public GoogleJSONRecognitionAlternative[] alternatives;
	}

	[Serializable]
	public class GoogleJSONRecognitionAlternative : TextAlternative
	{
		public string transcript;
		public double confidence;

		public override string Text {
			get {
				return transcript;
			}
			set {
				transcript = value;
			}
		}
	}
}

