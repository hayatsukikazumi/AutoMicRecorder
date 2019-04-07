using System;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// Response text data of SpeechToTextResult.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.0.0</version>
	/// <since>2018/05/20</since>
	[Serializable]
	public class TextAlternative
	{
		public virtual string Text {
			get;
			set;
		}
	}
}
