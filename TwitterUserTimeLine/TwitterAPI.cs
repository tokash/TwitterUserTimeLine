using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace TwitterUserTimeLine
{
    public class TwitterAPI
    {
        #region Members
        private TwitterAuthenticateResponse _Token;
	    #endregion

        public TwitterAPI(string iConsumerKey, string iConsumerSecret)
        {
            GetToken(iConsumerKey, iConsumerSecret);
        }

        private void GetToken(string iConsumerKey, string iConsumerSecret)
        {
            var oAuthUrl = "https://api.twitter.com/oauth2/token";

            // Do the Authenticate
            var authHeaderFormat = "Basic {0}";

            var authHeader = string.Format(authHeaderFormat,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(iConsumerKey) + ":" +
                Uri.EscapeDataString((iConsumerSecret)))
            ));

            var postBody = "grant_type=client_credentials";

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            authRequest.Headers.Add("Authorization", authHeader);
            authRequest.Method = "POST";
            authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (Stream stream = authRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            authRequest.Headers.Add("Accept-Encoding", "gzip");

            TwitterAuthenticateResponse twitAuthResponse;

            try
            {
                WebResponse authResponse = authRequest.GetResponse();

                // deserialize into an object    
                using (authResponse)
                {
                    using (var reader = new StreamReader(authResponse.GetResponseStream()))
                    {
                        var objectText = reader.ReadToEnd();
                        twitAuthResponse = JsonConvert.DeserializeObject<TwitterAuthenticateResponse>(objectText);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            _Token = twitAuthResponse;
        }

        public List<TweetObject> GetUserTimeLine(string iUserID, int iNumTweetsToGet)
        {
            List<TweetObject> timeline = new List<TweetObject>();
            //string since_id = string.Empty;
            string max_id = string.Empty;
            var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?user_id={0}&include_rts=1&exclude_replies=1&count=200";
            var timelineFormatCursor = "https://api.twitter.com/1.1/statuses/user_timeline.json?user_id={0}&max_id={1}&include_rts=1&exclude_replies=1&count=200";
            var timelineUrl = string.Format(timelineFormat, iUserID);
            WebResponse timeLineResponse = null;

            
            if (_Token != null)
            {
                // Do the timeline
                    
                //HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
                //var timelineHeaderFormat = "{0} {1}";
                //timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, _Token.token_type, _Token.access_token));
                //timeLineRequest.Method = "Get";

                try
                {
                    timeLineResponse = CallGetUserTimeline(timelineUrl, _Token);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("429"))
                    {
                        //need to wait 15 minutes
                        String message = string.Format("{0}: Rate Limit Reached for {1}, Sleeping for 15 minutes...", DateTime.Now, timelineUrl);
                        Console.WriteLine(message);
                        System.Threading.Thread.Sleep(900000);
                        Console.WriteLine(string.Format("{0}: Woke up...", DateTime.Now));

                        //calling again after 15 minutes
                        try
                        {
                            timeLineResponse = CallGetUserTimeline(timelineUrl, _Token);
                        }
                        catch (Exception)
                        {
                            
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }

                var timeLineJson = string.Empty;
                List<TweetObject> currTimeline = null;
                if (timeLineResponse != null)
                {
                    using (var reader = new StreamReader(timeLineResponse.GetResponseStream()))
                    {
                        timeLineJson = reader.ReadToEnd();
                    }

                    try
                    {
                        currTimeline = JsonConvert.DeserializeObject<List<TweetObject>>(timeLineJson);

                        //Adding current batch to timeline
                        if (currTimeline != null)
                        {
                            timeline.AddRange(currTimeline);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (timeLineResponse != null)
                {
                    while (timeline.Count < iNumTweetsToGet && currTimeline != null && currTimeline.Count > 1)
                    {
                        if (timeLineResponse.Headers["X-Rate-Limit-Remaining"] != null)
                        {
                            int XRateLimitRemaining = Int32.Parse(timeLineResponse.Headers["X-Rate-Limit-Remaining"]);
                            Console.WriteLine(string.Format("Url:{0}\nRemaining calls: {1}", timelineUrl, XRateLimitRemaining));

                            if (XRateLimitRemaining == 0)
                            {
                                //wait 15 minutes
                                String message = string.Format("{0}: Rate Limit Reached for {1}, Sleeping for 15 minutes...", DateTime.Now, timelineUrl);
                                Console.WriteLine(message);
                                System.Threading.Thread.Sleep(900000);
                                Console.WriteLine(string.Format("{0}: Woke up...", DateTime.Now));
                            }
                        }

                        List<TweetObject> tmpTimeLine = currTimeline.OrderBy(x => x.id).ToList();
                        max_id = tmpTimeLine[0].id.ToString();
                        currTimeline.Clear();

                        timelineUrl = string.Format(timelineFormatCursor, iUserID, max_id);
                        //timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
                        //timelineHeaderFormat = "{0} {1}";
                        //timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, _Token.token_type, _Token.access_token));
                        //timeLineRequest.Method = "Get";
                        //timeLineResponse = timeLineRequest.GetResponse();

                        try
                        {
                            timeLineResponse = CallGetUserTimeline(timelineUrl, _Token);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("429"))
                            {
                                //need to wait 15 minutes
                                String message = string.Format("{0}: Rate Limit Reached for {1}, Sleeping for 15 minutes...", DateTime.Now, timelineUrl);
                                Console.WriteLine(message);
                                System.Threading.Thread.Sleep(900000);
                                Console.WriteLine(string.Format("{0}: Woke up...", DateTime.Now));

                                //calling again after 15 minutes
                                try
                                {
                                    timeLineResponse = CallGetUserTimeline(timelineUrl, _Token);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }

                        if (timeLineResponse != null)
                        {
                            using (var reader = new StreamReader(timeLineResponse.GetResponseStream()))
                            {
                                timeLineJson = reader.ReadToEnd();
                            }
                        }

                        try
                        {
                            currTimeline = JsonConvert.DeserializeObject<List<TweetObject>>(timeLineJson);

                            //Adding current batch to timeline
                            if (currTimeline != null)
                            {
                                timeline.AddRange(currTimeline.Where(x => x.id != max_id));
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            

            return timeline;
        }

        private WebResponse CallGetUserTimeline(string iRequestURL, TwitterAuthenticateResponse iToken)
        {
            HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(iRequestURL);
            var timelineHeaderFormat = "{0} {1}";
            timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, iToken.token_type, iToken.access_token));
            timeLineRequest.Method = "Get";
            WebResponse timeLineResponse = null;
            bool isSucceeded = false;
            int failureCounter = 0;

            while (!isSucceeded && failureCounter < 5)
            {
                try
                {
                    timeLineResponse = timeLineRequest.GetResponse();
                    isSucceeded = true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("401"))
                    {
                        Console.WriteLine(string.Format("{0}: timeline is protected for url:{1}, cannot get timeline ", DateTime.Now, iRequestURL));
                        isSucceeded = true;
                    }
                    else if (ex.Message.Contains("403"))
                    {
                        Console.WriteLine(string.Format("{0}: Twitter server refused or access is not allowed : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        isSucceeded = true;
                    }
                    else if (ex.Message.Contains("404"))
                    {
                        Console.WriteLine(String.Format("{0}: The URI requested is invalid or the resource requested, such as a user, does not exists : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        isSucceeded = true;
                    }
                    else if (ex.Message.Contains("406"))
                    {
                        Console.WriteLine(String.Format("{0}: Invalid format specified in the request. : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        isSucceeded = true;
                    }
                    else if (ex.Message.Contains("410"))
                    {
                        Console.WriteLine(String.Format("{0}: This resource is gone, requests to this endpoint will yield no results from now on. : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        isSucceeded = true;
                    }
                    else if (ex.Message.Contains("500"))
                    {
                        Console.WriteLine(String.Format("{0}: Twitter server returned Internal Server Error : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        failureCounter++;
                    }
                    else if (ex.Message.Contains("502"))
                    {
                        Console.WriteLine(String.Format("{0}: Twitter servers are down or being upgraded: {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        throw ex;
                    }
                    else if (ex.Message.Contains("503"))
                    {
                        Console.WriteLine(String.Format("{0}: Twitter servers are up but overloaded with requests : {1},\nData will not be retrieved for: {2}", DateTime.Now, ex.Message.ToString(), iRequestURL));
                        failureCounter++;
                        System.Threading.Thread.Sleep(1000);
                    }
                    else if (ex.Message.Contains("429"))
                    {
                        throw;
                    }
                } 
            }

            return timeLineResponse;
        }
    }


}
