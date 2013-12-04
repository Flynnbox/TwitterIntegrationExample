using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TwitterIntegrationExample.Twitter
{
	[DataContract]
	public class TwitterErrorResponse
	{
		[DataMember(Name = "errors")]
		public List<TwitterError> Errors { get; set; }
	}

	[DataContract]
	public class TwitterError
	{
		[DataMember(Name = "code")]
		public int Code { get; set; }

		[DataMember(Name = "message")]
		public string Message { get; set; }
	}
}