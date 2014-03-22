using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Web.Helpers;

namespace TwitterUserTimeLine
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitterAPI twitterAPI = new TwitterAPI("lVVcDevyLOZcL2dqy4lL0g", "WZJxCCR2SY87SAVEJqBGBE7I5JOdGUYSlywTxMQdo");


            List<TweetObject> tweets = twitterAPI.GetUserTimeLine("183033795", 3200);
        }
    }

    
}
