using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace TwitterIntegrationExample.Twitter
{
	[DataContract]
	public class TwitterTweetSearchResponse
	{
		[DataMember(Name = "statuses")]
		public List<TwitterStatus> Statuses { get; set; }

		[DataMember(Name = "search_metadata")]
		public TwitterSearchMetadata SearchMetadata { get; set; }

		/*
		"search_metadata": {
			"max_id": 250126199840518145,
			"since_id": 24012619984051000,
			"refresh_url": "?since_id=250126199840518145&q=%23freebandnames&result_type=mixed&include_entities=1",
			"next_results": "?max_id=249279667666817023&q=%23freebandnames&count=4&include_entities=1&result_type=mixed",
			"count": 4,
			"completed_in": 0.035,
			"since_id_str": "24012619984051000",
			"query": "%23freebandnames",
			"max_id_str": "250126199840518145"
		}
		*/
	}

	[DataContract]
	public class TwitterSearchMetadata
	{
		[DataMember(Name = "query")]
		public string Query { get; set; }

		[DataMember(Name = "next_results")]
		public string NextResultsUrl { get; set; }
	}

	[DataContract]
	public class TwitterStatus
	{
		[DataMember(Name = "id_str")]
		public string Id { get; set; }

		[DataMember(Name = "created_at")]
		public string CreatedAtString { get; set; }

		public DateTime CreatedAt
		{
			get { return DateTime.ParseExact(CreatedAtString, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture); }
		}

		[DataMember(Name = "text")]
		public string Text { get; set; }

		[DataMember(Name = "user")]
		public TwitterUser User { get; set; }
	}

	[DataContract]
	public class TwitterUser
	{
		[DataMember(Name = "id_str")]
		public string Id { get; set; }

		[DataMember(Name = "screen_name")]
		public string ScreenName { get; set; }

		[DataMember(Name = "profile_image_url")]
		public string ProfileImageUrl { get; set; }
	}
}