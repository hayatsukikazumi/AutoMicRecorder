using System;

namespace hayatsukikazumi.amr
{
	/// <summary>
	/// JSON error response of Google Cloud API.
	/// </summary>
	/// <author>Hayatsukikazumi</author>
	/// <version>2.0.0</version>
	/// <since>2018/05/20</since>
	[Serializable]
	public class GoogleJSONResponseError
	{
		public GoogleJSONError error;
	}

	[Serializable]
	public class GoogleJSONError
	{
		public int code;
		public string message;
	}
}
