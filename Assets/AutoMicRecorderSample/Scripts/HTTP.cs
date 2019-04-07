using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Alternative class of 'Uniweb' asset, necessary to work 'Speech-to-text' asset.
/// </summary>
/// <author>Hayatsukikazumi</author>
/// <version>2.1.0</version>
/// <since>2017/10/15</since>
namespace HTTP
{
	public class Request
	{
		UnityWebRequest req;
		HeaderWrapper header;
		ResponseWrapper res;

		public HeaderWrapper headers {
			get { return header; }
		}

		public ResponseWrapper response {
			get { return res; }
		}

		public string Text {
			set { req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(value)); }
		}

		public byte[] Bytes {
			set { req.uploadHandler = new UploadHandlerRaw(value); }
		}

		public Request(string method, string url) {
			req = new UnityWebRequest(url, method);
			header = new HeaderWrapper(req);
		}

		public AsyncOperation Send() {
			req.downloadHandler = new DownloadHandlerBuffer();
			res = new ResponseWrapper(req);
			return req.Send();
		}

		public bool isDone {
			get { return req.isDone; }
		}

		public class HeaderWrapper {

			private UnityWebRequest _req;
			public HeaderWrapper(UnityWebRequest req) {
				_req = req;
			}

			public void Add(string name, string value) {
				_req.SetRequestHeader(name, value);
			}
		}

		public class ResponseWrapper {

			private UnityWebRequest _req;
			public ResponseWrapper(UnityWebRequest req) {
				_req = req;
			}

			public string Text {
				get { return Encoding.UTF8.GetString (_req.downloadHandler.data); }
			}

			public int status {
				get {
					int ret = (int)_req.responseCode;
					if (ret <= 0 && (_req.isHttpError || _req.isNetworkError)) return 400;

					return ret;
				}
			}
		}
	}
}

