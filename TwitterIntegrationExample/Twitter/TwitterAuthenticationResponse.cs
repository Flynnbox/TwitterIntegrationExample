using System.Runtime.Serialization;

namespace TwitterIntegrationExample.Twitter
{
	[DataContract]
	public class TwitterGetTokenResponse
	{
		[DataMember(Name = "token_type")]
		public string TokenType { get; set; }

		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }
	}

	[DataContract]
	public class TwitterInvalidateTokenResponse
	{
		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }
	}
}