using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;

//using log4net;

namespace TwitterIntegrationExample.Twitter
{
	public class TwitterSearch
	{
		private const int MAX_QUERY_LENGTH = 1000;
		private static readonly string twitterUrlSearch;

		static TwitterSearch()
		{
			twitterUrlSearch = ConfigurationManager.AppSettings["twitterUrlSearch"];
		}

		public static TwitterTweetSearchResponse SearchTweets(IEnumerable<string> accountNames, IEnumerable<string> hashtags, int resultsPerPage, string maxId = null)
		{
			var url = BuildSearchUrl(accountNames, hashtags, resultsPerPage, maxId);
			var parsedResponse = TwitterApplicationAuth.GetResponse<TwitterTweetSearchResponse>(url, null);
			return parsedResponse;
		}

		private static string BuildSearchUrl(IEnumerable<string> accountNames, IEnumerable<string> hashtags, int resultsPerPage, string maxId)
		{
			bool isFirstItem = true;
			StringBuilder sb = new StringBuilder(twitterUrlSearch);
			sb.Append("q=");

			if (accountNames != null)
			{
				foreach (var accountName in accountNames)
				{
					if (!isFirstItem)
					{
						sb.Append(Uri.EscapeDataString(" OR "));
					}
					if (accountName.IndexOf("@", StringComparison.OrdinalIgnoreCase) == -1)
					{
						sb.Append(Uri.EscapeDataString("@"));
					}
					sb.Append(Uri.EscapeDataString(accountName));
					isFirstItem = false;
				}
			}

			if (hashtags != null)
			{
				foreach (var hashtag in hashtags)
				{
					if (!isFirstItem)
					{
						sb.Append(Uri.EscapeDataString(" OR "));
					}
					if (hashtag.IndexOf("#", StringComparison.OrdinalIgnoreCase) == -1)
					{
						sb.Append(Uri.EscapeDataString("#"));
					}
					sb.Append(Uri.EscapeDataString(hashtag));
					isFirstItem = false;
				}
			}

			sb.Append("&count=");
			sb.Append(resultsPerPage > 100 || resultsPerPage < 0 ? 100 : resultsPerPage);

			if (!string.IsNullOrEmpty(maxId))
			{
				sb.Append("&maxid=");
				sb.Append(maxId);
			}

			if (sb.Length > MAX_QUERY_LENGTH)
			{
				throw new Exception(string.Format("Twitter query exceeds limit of {0} character length: {1}", MAX_QUERY_LENGTH, sb));
			}
			return sb.ToString();
		}
	}
}