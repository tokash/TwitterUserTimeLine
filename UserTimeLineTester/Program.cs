using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterUserTimeLine;

namespace UserTimeLineTester
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitterAPI twitterAPI = new TwitterAPI("lVVcDevyLOZcL2dqy4lL0g", "WZJxCCR2SY87SAVEJqBGBE7I5JOdGUYSlywTxMQdo");


            List<TweetObject> tweets = twitterAPI.GetUserTimeLineInTimeFrame("16589206", new TimeSpan(24, 0, 0));
        }
    }
}
