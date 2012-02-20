using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Async;

namespace MoviesBroker.Services
{
    public abstract class TaskService<T>
    {
        public Task<T> Task { get; private set; }

        protected void SetResponseTask(string url)
        {
            var requestTask = WebRequest.Create(url).WebRequestTaskAsync();

            this.Task = requestTask.ContinueWith(_ =>
                                           {
                                               var imdbResponse =
                                                   (HttpWebResponse) requestTask.Result;
                                               var jsonMaster = new JavaScriptSerializer();
                                               return
                                                   jsonMaster.Deserialize<T>(
                                                       new StreamReader(imdbResponse.GetResponseStream()).ReadToEnd());
                                           });
        }

        protected void SetCachedResponseTask(string url)
        {
            if (Caching.UrlRequestCache<T>.Instance.Contains(url))
            {
                this.Task = Caching.UrlRequestCache<T>.Instance.Get(url);
            }
            else
            {
                SetResponseTask(url);
                this.Task = Caching.UrlRequestCache<T>.Instance.TryAdd(url, this.Task);
            }
        }

        public T Result
        {
            get { return Task.Result; }
        }
    }
}