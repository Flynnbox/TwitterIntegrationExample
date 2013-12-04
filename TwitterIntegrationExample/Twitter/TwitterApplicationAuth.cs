using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

//using log4net;

namespace TwitterIntegrationExample.Twitter
{
	/// <summary>
	/// Note that because of the choice to read the consumerKey and consumerSecret out of the config file, there is no way within the same application to authenticate different Twitter apps. 
	/// Consider a typed refactoring if this becomes necessary.
	/// </summary>
	public static class TwitterApplicationAuth
	{
		private const string POST_DATA_GET_TOKEN = "grant_type=client_credentials";
		private const string POST_DATA_INVALIDATE_TOKEN = "access_token={0}";
		private const string TOKEN_TYPE_BEARER = "bearer";

		//private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int twitterApiTimeout;
		private static readonly string twitterUrlGetToken;
		private static readonly string twitterUrlInvalidateToken;
		private static readonly string bearerTokenCredentials;
		private static string bearerToken;

		static TwitterApplicationAuth()
		{
			string timeout = ConfigurationManager.AppSettings["twitterApiTimeout"];
			if (!int.TryParse(timeout, out twitterApiTimeout))
			{
				twitterApiTimeout = 5000;
			}

			twitterUrlGetToken = ConfigurationManager.AppSettings["twitterUrlGetToken"];
			twitterUrlInvalidateToken = ConfigurationManager.AppSettings["twitterUrlInvalidateToken"];
			var consumerKey = HttpUtility.UrlEncode(ConfigurationManager.AppSettings["twitterConsumerKey"]);
			var consumerSecret = HttpUtility.UrlEncode(ConfigurationManager.AppSettings["twitterConsumerSecret"]);
			bearerTokenCredentials = Base64Encode(String.Format("{0}:{1}", consumerKey, consumerSecret));
		}

		public static void GetBearerToken()
		{
			if (IsAuthenticated())
			{
				//if (log.IsWarnEnabled)
				//{
				//	log.WarnFormat("Twitter API - Did not retrieve new bearer token as bearer token already exists.");
				//}
				return;
			}

			var request = CreateTwitterAuthenticationWebRequest(twitterUrlGetToken, POST_DATA_GET_TOKEN);

			using (HttpWebResponse response = request.GetResponseNoException())
			{
				var authResponse = ParseResponse<TwitterGetTokenResponse>(response, twitterUrlGetToken);

				if (!authResponse.TokenType.Equals(TOKEN_TYPE_BEARER, StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception(string.Format("HttpWebResponse from {0} has invalid token_type of '{1}' instead of '{2}'", twitterUrlGetToken, authResponse.TokenType, TOKEN_TYPE_BEARER));
				}

				//if (log.IsInfoEnabled)
				//{
				//	log.InfoFormat("Twitter API - Got Access Token");
				//}

				bearerToken = authResponse.AccessToken;
			}
		}

		public static void InvalidateBearerToken()
		{
			if (!IsAuthenticated())
			{
				//if (log.IsWarnEnabled)
				//{
				//	log.WarnFormat("Twitter API - Cannot invalidate token as no bearer token exists.");
				//}
				return;
			}

			var request = CreateTwitterAuthenticationWebRequest(twitterUrlInvalidateToken, string.Format(POST_DATA_INVALIDATE_TOKEN, bearerToken));

			using (HttpWebResponse response = request.GetResponseNoException())
			{
				var authResponse = ParseResponse<TwitterInvalidateTokenResponse>(response, twitterUrlInvalidateToken);

				if (!authResponse.AccessToken.Equals(bearerToken, StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception(string.Format("HttpWebResponse from {0} has invalid access_token of '{1}' instead of '{2}'", twitterUrlInvalidateToken, authResponse.AccessToken, bearerToken));
				}

				//if (log.IsInfoEnabled)
				//{
				//	log.InfoFormat("Twitter API - Invalidated Access Token");
				//}

				bearerToken = null;
			}
		}

		public static bool IsAuthenticated()
		{
			return !String.IsNullOrEmpty(bearerToken);
		}

		/// <summary>
		/// Creates an authenticated web request to twitter and returns the parsed response
		/// </summary>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static T GetResponse<T>(string url, string data) where T : class
		{
			if (!IsAuthenticated())
			{
				GetBearerToken();
			}
			var request = CreateTwitterWebRequest(url, data);

			//if (log.IsInfoEnabled)
			//{
			//	log.InfoFormat("Making http request to {0}", url);
			//}

			using (HttpWebResponse response = request.GetResponseNoException())
			{
				return ParseResponse<T>(response, url);
			}
		}

		private static HttpWebRequest CreateTwitterAuthenticationWebRequest(string url, string data)
		{
			return CreateTwitterWebRequest(url, data, "POST", "Basic " + bearerTokenCredentials);
		}

		private static HttpWebRequest CreateTwitterWebRequest(string url, string data)
		{
			return CreateTwitterWebRequest(url, data, "GET", "Bearer " + bearerToken);
		}

		private static HttpWebRequest CreateTwitterWebRequest(string url, string data, string method, string authorizationHeader)
		{
			//if (log.IsDebugEnabled)
			//{
			//	log.DebugFormat("Creating HttpWebRequest with url of {0}", url);
			//}

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;
			request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
			request.Headers.Add("Authorization", authorizationHeader);
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.Timeout = twitterApiTimeout;

			if (!string.IsNullOrEmpty(data))
			{
				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] encodedData = encoding.GetBytes(data);
				request.ContentLength = encodedData.Length;

				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(encodedData, 0, encodedData.Length);
				}
			}

			//if (log.IsDebugEnabled)
			//{
			//	log.DebugFormat("Created HttpWebRequest with url of {0}", request.Address.AbsoluteUri);
			//}
			return request;
		}

		private static T ParseResponse<T>(HttpWebResponse response, string url) where T: class
		{
			var type = typeof (T);
			if (response == null)
			{
				throw new Exception(string.Format("HttpWebRequest to {0} returned null response.", url));
			}

			if (response.StatusCode != HttpStatusCode.OK)
			{
				//if (log.IsErrorEnabled)
				//{
				//	log.ErrorFormat("HttpWebRequest to {0} returned server error (HTTP {1}: {2}).", url, response.StatusCode, response.StatusDescription);
				//	var errorResponse = GetTypedResponse<TwitterErrorResponse>(response.GetResponseStream(), url, typeof(TwitterErrorResponse));
				//	var messages = string.Join(";\n", errorResponse.Errors.Select(e => e.Message).ToArray());
				//	log.ErrorFormat("Twitter API - Error Messages: {0}", messages);
				//}
				throw new Exception(string.Format("HttpWebRequest to {0} returned server error (HTTP {1}: {2}).", url, response.StatusCode, response.StatusDescription));
			}
			return GetTypedResponse<T>(response.GetResponseStream(), url, type);
		}

		private static T GetTypedResponse<T>(Stream responseStream, string url, Type type) where T : class
		{
			if (responseStream == null)
			{
				throw new Exception(string.Format("HttpWebResponse from {0} returned a null response stream", url));
			}
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(type);
			var typedResponse = jsonSerializer.ReadObject(responseStream) as T;

			if (typedResponse == null)
			{
				throw new Exception(string.Format("HttpWebResponse from {0} could not be parsed to {1}", url, type.Name));
			}
			return typedResponse;
		}

		private static string Base64Encode(string toEncode)
		{
			byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
			return Convert.ToBase64String(toEncodeAsBytes);
		}
	}

	/// <summary>
	/// Guarantees a HttpWebRequest will not throw an exception but instead 
	/// return a null or populated response object
	/// </summary>
	public static class HttpWebRequestExtension
	{
		public static HttpWebResponse GetResponseNoException(this HttpWebRequest request)
		{
			try
			{
				return request.GetResponse() as HttpWebResponse;
			}
			catch (WebException ex)
			{
				var response = ex.Response as HttpWebResponse;
				return response;
			}
		}
	}
}