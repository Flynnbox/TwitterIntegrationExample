using TwitterIntegrationExample.Twitter;

namespace TwitterIntegrationExample
{
	class Program
	{
		static void Main(string[] args)
		{
			//TODO update app.config with twitter consumer key, consumer secret, and the values below
			TwitterSearch.SearchTweets([TwitterScreenNames], [TwitterHashTags], [ResultsPerPage])
		}
	}
}
