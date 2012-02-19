using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Threading;
using Async;

namespace MovieInfo
{
    using Models;

    public class MovieInfoHandler : IHttpAsyncHandler
    {
        private const int MAX_RETRIES = 8;
        private const string BING_KEY = "BEBF1591DC156190D8BA4CB046FB771173332D94";

        private static readonly Random _rand = new Random();

        public bool IsReusable
        {
            get { return false; }
        }

        private void ReplyError(HttpStatusCode statusCode, string text, HttpResponse response)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/html";
            response.Write(String.Format("<html><title>MovieInfo : ERROR</title><body><p>{0}</p></body></html>", text));
            for (int i = 0; i < 85; ++i) response.Write("&nbsp;");
        }

        public void ProcessRequest(HttpContext context)
        {

            if (context.Request.Path != "/")
            {
                ReplyError(HttpStatusCode.NotFound, "Resource does not exist", context.Response);
                return;
            }

            if (context.Request.QueryString["t"] == null)
            {
                ReplyError(HttpStatusCode.BadRequest, "Requests must indicate a movie via parameter <b>t=<i>movie</i></b>", context.Response);
                return;
            }

            string imdbRequestUri = null;
            if (context.Request.QueryString["y"] == null) {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}", context.Request.QueryString["t"]);
            } else {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}&y={1}", context.Request.QueryString["t"], context.Request.QueryString["y"]);
            }

            WebRequest imdbRequest = (HttpWebRequest)WebRequest.Create(imdbRequestUri);

            var imdbtask = Task.Factory.FromAsync(imdbRequest.BeginGetResponse
                                   , (Func<IAsyncResult, WebResponse>) imdbRequest.EndGetResponse
                                   , null);
            
            HttpWebResponse imdbResponse = (HttpWebResponse)imdbRequest.GetResponse();

            if (imdbResponse.StatusCode == HttpStatusCode.OK) {
                JavaScriptSerializer jsonMaster = new JavaScriptSerializer();
                IMDbObj imdbObj = jsonMaster.Deserialize<IMDbObj>(new StreamReader(imdbResponse.GetResponseStream()).ReadToEnd());
                if (imdbObj != null && imdbObj.Response == "True") {
                    if (imdbObj.Plot != "N/A")
                    {
                        string bingRequestUri = String.Format("http://api.bing.net/json.aspx?AppId={0}&Query={1}&Sources=Translation&Version=2.2&Translation.SourceLanguage={2}&Translation.TargetLanguage={3}", BING_KEY, imdbObj.Plot, "en", "pt");

                        for (int retries = 0; retries < MAX_RETRIES; ++retries) {

                            HttpWebRequest bingRequest = (HttpWebRequest)WebRequest.Create(bingRequestUri);
                            HttpWebResponse bingResponse = (HttpWebResponse)bingRequest.GetResponse();

                            if (bingResponse.StatusCode == HttpStatusCode.OK)
                            {
                                BingObj bingObj = jsonMaster.Deserialize<BingObj>(new StreamReader(bingResponse.GetResponseStream()).ReadToEnd());
                                if (bingObj.SearchResponse.Translation != null)
                                {
                                    imdbObj.Plot = bingObj.SearchResponse.Translation.Results[0].TranslatedTerm;
                                    break;
                                }
                            }

                            Thread.Sleep(1000 + 1000 * retries + _rand.Next(2000));
                        }
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = "text/plain; charset=utf-8"; // In the final version use "application/json; charset=utf-8"
                    string jsonData = jsonMaster.Serialize(imdbObj);
                    var writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write(jsonData);
                    writer.Flush();
                    return;
                }
            }

            ReplyError(HttpStatusCode.NotFound, "Movie not found", context.Response);
        }

        private class MovieInfoResult:IAsyncResult
        {
            public AsyncCallback Callback { get; set; }
            public MovieInfoResult(AsyncCallback cb, object state)
            {
                IsCompleted = false;
                AsyncState = state;
                Callback = cb;
                AsyncWaitHandle = new ManualResetEvent(false);
            }

            public bool IsCompleted { get; set; }

            public WaitHandle AsyncWaitHandle { get; set; }

            public object AsyncState { get; set; }

            public bool CompletedSynchronously
            {
                get { return false; }
            }
        }


        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var miar = new MovieInfoResult(cb, extraData);
            ProcessRequesIterator(context, miar).Run();
            return miar;
        }

        private IEnumerable<Task> ProcessRequesIterator(HttpContext context, MovieInfoResult miar)
        {

            if (context.Request.Path != "/")
            {
                ReplyError(HttpStatusCode.NotFound, "Resource does not exist", context.Response);
                yield return new Task(()=>
                                                       {
                                                           miar.IsCompleted = true;
                                                           ((ManualResetEvent)miar.AsyncWaitHandle).Set();
                                                           miar.Callback(miar);
                                                       });
                yield break;
            }


            if (context.Request.QueryString["t"] == null)
            {
                ReplyError(HttpStatusCode.BadRequest, "Requests must indicate a movie via parameter <b>t=<i>movie</i></b>", context.Response);
                yield return new Task(() =>
                                                       {
                                                           miar.IsCompleted = true;
                                                           ((ManualResetEvent)miar.AsyncWaitHandle).Set();
                                                           miar.Callback(miar);
                                                       });
                yield break;
            }

            string imdbRequestUri = null;
            if (context.Request.QueryString["y"] == null)
            {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}", context.Request.QueryString["t"]);
            }
            else
            {
                imdbRequestUri = String.Format("http://imdbapi.com/?t={0}&y={1}", context.Request.QueryString["t"], context.Request.QueryString["y"]);
            }

            WebRequest imdbRequest = WebRequest.Create(imdbRequestUri);

            var imdbtask = Task.Factory.FromAsync(imdbRequest.BeginGetResponse
                                   , (Func<IAsyncResult, WebResponse>)imdbRequest.EndGetResponse
                                   , null);
            yield return imdbtask;

            HttpWebResponse imdbResponse = (HttpWebResponse)imdbtask.Result;

            if (imdbResponse.StatusCode == HttpStatusCode.OK)
            {
                JavaScriptSerializer jsonMaster = new JavaScriptSerializer();
                IMDbObj imdbObj = jsonMaster.Deserialize<IMDbObj>(new StreamReader(imdbResponse.GetResponseStream()).ReadToEnd());
                if (imdbObj != null && imdbObj.Response == "True")
                {
                    if (imdbObj.Plot != "N/A")
                    {
                        string bingRequestUri = String.Format("http://api.bing.net/json.aspx?AppId={0}&Query={1}&Sources=Translation&Version=2.2&Translation.SourceLanguage={2}&Translation.TargetLanguage={3}", BING_KEY, imdbObj.Plot, "en", "pt");

                        for (int retries = 0; retries < MAX_RETRIES; ++retries)
                        {

                            WebRequest bingRequest = (HttpWebRequest)WebRequest.Create(bingRequestUri);

                            var bingTask = Task.Factory.FromAsync(bingRequest.BeginGetResponse
                                                                  ,
                                                                  (Func<IAsyncResult, WebResponse>)
                                                                  bingRequest.EndGetResponse
                                                                  , null);

                            yield return bingTask;

                            HttpWebResponse bingResponse = (HttpWebResponse) bingTask.Result;

                            if (bingResponse.StatusCode == HttpStatusCode.OK)
                            {
                                BingObj bingObj = jsonMaster.Deserialize<BingObj>(new StreamReader(bingResponse.GetResponseStream()).ReadToEnd());
                                if (bingObj.SearchResponse.Translation != null)
                                {
                                    imdbObj.Plot = bingObj.SearchResponse.Translation.Results[0].TranslatedTerm;
                                    break;
                                }
                            }

                            int retrieslocal = retries;
                            yield return new Task(() =>
                                                                   {
                                                                       new Timer((_) => { }
                                                                                 , null, 000 + 1000*retrieslocal +
                                                                                         _rand.Next(2000), 0
                                                                           );
                                                                   });

                        }
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = "text/plain; charset=utf-8"; // In the final version use "application/json; charset=utf-8"
                    string jsonData = jsonMaster.Serialize(imdbObj);
                    var writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write(jsonData);
                    writer.Flush();
                    miar.IsCompleted = true;
                    ((ManualResetEvent)miar.AsyncWaitHandle).Set();
                    miar.Callback(miar);
                    yield break;
                }
            }

            ReplyError(HttpStatusCode.NotFound, "Movie not found", context.Response);
        }


        public void EndProcessRequest(IAsyncResult result)
        {

        }
    }
}
