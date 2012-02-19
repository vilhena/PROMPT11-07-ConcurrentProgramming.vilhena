using System;
using System.Web;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Threading;

namespace MovieInfo
{
    using Models;

    public class MovieInfoHandler : IHttpAsyncHandler
    {
        private const int MAX_RETRIES = 8;
        private const string BING_KEY = "BEBF1591DC156190D8BA4CB046FB771173332D94";

        private static readonly Random _rand = new Random();

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get { return false; }
        }


        private class MyHttpAsyncResult : IAsyncResult
        {
            private readonly AsyncCallback _callback;

            public MyHttpAsyncResult(HttpWebRequest httpWebRequest, HttpContext context, AsyncCallback callback, object extraData)
            {
                HttpWebRequest = httpWebRequest;
                AsyncState = extraData;
                _callback = callback;
                Context = context;
                AsyncWaitHandle = new ManualResetEvent(false);

            }

            public HttpContext Context { get; set; }
            public bool IsCompleted { get; set; }
            public HttpWebRequest HttpWebRequest { get; set; }

            public AsyncCallback Callback { get { return _callback; } }

            public WaitHandle AsyncWaitHandle { get; private set; }

            public object AsyncState { get; set; }

            public bool CompletedSynchronously
            {
                get { return false; }
            }
        }

        private void ReplyError(HttpStatusCode statusCode, string text, HttpResponse response)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/html";
            response.Write(String.Format("<html><title>MovieInfo : ERROR</title><body><p>{0}</p></body></html>", text));
            for (int i = 0; i < 85; ++i) response.Write("&nbsp;");
        }
        

        private void ImdbCallback(IAsyncResult ar)
        {
            var httpCb = ar.AsyncState as MyHttpAsyncResult;
            

            if (httpCb != null)
            {
                var imdbResponse = httpCb.HttpWebRequest.EndGetResponse(ar) as HttpWebResponse;
                var context = httpCb.Context;

                //do stuff

                if (imdbResponse.StatusCode == HttpStatusCode.OK)
                {
                    JavaScriptSerializer jsonMaster = new JavaScriptSerializer();
                    IMDbObj imdbObj = jsonMaster.Deserialize<IMDbObj>(new StreamReader(imdbResponse.GetResponseStream()).ReadToEnd());
                    if (imdbObj != null && imdbObj.Response == "True")
                    {
                        if (imdbObj.Plot != "N/A")
                        {
                            string bingRequestUri = String.Format("http://api.bing.net/json.aspx?AppId={0}&Query={1}&Sources=Translation&Version=2.2&Translation.SourceLanguage={2}&Translation.TargetLanguage={3}", BING_KEY, imdbObj.Plot, "en", "pt");

                            
                            //for (int retries = 0; retries < MAX_RETRIES ; ++retries)
                            //{

                                HttpWebRequest bingRequest = (HttpWebRequest)WebRequest.Create(bingRequestUri);

                                var bAR = new MyHttpAsyncResult(bingRequest, httpCb.Context, (_)=>
                                                                                                           {
                                                                                                               context.Response.StatusCode = (int)HttpStatusCode.OK;
                                                                                                               context.Response.ContentType = "text/plain; charset=utf-8"; // In the final version use "application/json; charset=utf-8"
                                                                                                               string jsonData = jsonMaster.Serialize(imdbObj);
                                                                                                               var writer = new StreamWriter(context.Response.OutputStream);
                                                                                                               writer.Write(jsonData);
                                                                                                               writer.Flush();

                                                                                                               httpCb.IsCompleted = true;
                                                                                                               ((ManualResetEvent)httpCb.AsyncWaitHandle).Set();
                                                                                                               httpCb.Callback(httpCb);
                                                                                                               return;
                                                                                                           },
                                                                httpCb.AsyncState);

                                var bingResponseAR = bingRequest.BeginGetResponse((a) =>
                                                                                      {
                                                                                          var final =
                                                                                              a.AsyncState as
                                                                                              MyHttpAsyncResult;

                                                                                          var bingResponse = final.HttpWebRequest.EndGetResponse(a) as HttpWebResponse;

                                                                                          if (bingResponse.StatusCode == HttpStatusCode.OK)
                                                                                          {

                                                                                              BingObj bingObj = jsonMaster.Deserialize<BingObj>(new StreamReader(bingResponse.GetResponseStream()).ReadToEnd());
                                                                                              if (bingObj.SearchResponse.Translation != null)
                                                                                              {
                                                                                                  imdbObj.Plot = bingObj.SearchResponse.Translation.Results[0].TranslatedTerm;
                                                                                                  
                                                                                                  //Ends for
                                                                                                  final.IsCompleted =
                                                                                                      true;
                                                                                                  ((ManualResetEvent)
                                                                                                   final.AsyncWaitHandle)
                                                                                                      .Set();
                                                                                                  final.Callback(final);
                                                                                                  return;
                                                                                              }
                                                                                          }


                                                                                      }, bAR);

                                //2
                                //HttpWebResponse bingResponse = (HttpWebResponse)bingRequest.GetResponse();


                                //3
                                //Thread.Sleep(1000 + 1000 * retries + _rand.Next(2000));
                            //}
                        }
                        
                    }
                }
                else //TODO: change this
                {
                    ReplyError(HttpStatusCode.NotFound, "Movie not found", context.Response);   
                }
                

                
            }
            throw new NotImplementedException();

            

        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            //if (context.Request.Path != "/")
            //{
            //    ReplyError(HttpStatusCode.NotFound, "Resource does not exist", context.Response);
            //    return new HttpAsyncResult();
            //}

            //if (context.Request.QueryString["t"] == null)
            //{
            //    //context.Request.QueryString["t"] = "matrix";
            //    ReplyError(HttpStatusCode.BadRequest, "Requests must indicate a movie via parameter <b>t=<i>movie</i></b>", context.Response);
            //    return;
            //}

            var t = "matrix";

            string imdbRequestUri = null;
            if (context.Request.QueryString["y"] == null)
            {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}", t);
            }
            else
            {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}&y={1}", t, context.Request.QueryString["y"]);
            }

            HttpWebRequest imdbRequest = (HttpWebRequest)WebRequest.Create(imdbRequestUri);

            var httpAR = new MyHttpAsyncResult(imdbRequest, context, cb, extraData);

            imdbRequest.BeginGetResponse(ImdbCallback, httpAR);



            //1
            //HttpWebResponse imdbResponse = (HttpWebResponse)imdbRequest.GetResponse();


            return httpAR;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            //throw new NotImplementedException();
        }
    }
}
