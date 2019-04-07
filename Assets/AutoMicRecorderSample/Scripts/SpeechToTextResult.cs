using System;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// Response result of SpeechToTextService interface.
	/// </summary>
	public class SpeechToTextResult
	{
		public bool IsFinal
		{
			get;
			set;
		}

		public TextAlternative[] TextAlternatives
		{
			get;
			set;
		}
		public SpeechToTextResult()
		{
			IsFinal = true;
			TextAlternatives = new TextAlternative[0];
		}

		public SpeechToTextResult(bool isFinal, string text) {
			IsFinal = isFinal;
			TextAlternatives = new TextAlternative[] { new TextAlternative() };
			TextAlternatives[0].Text = text;
		}
	}
}

