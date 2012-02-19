using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MoviesBroker.Services
{
    public abstract class TaskService<T>
    {
        protected Task<WebResponse> _task;
        public Task Task { get { return _task; } }

        public T Result
        {
            get
            {
                var imdbResponse = (HttpWebResponse)_task.Result;
                JavaScriptSerializer jsonMaster = new JavaScriptSerializer();
                return jsonMaster.Deserialize<T>(new StreamReader(imdbResponse.GetResponseStream()).ReadToEnd());
            }
        }
    }
}