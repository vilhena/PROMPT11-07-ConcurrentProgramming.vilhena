using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using MoviesBroker.Models.ExternalModels;

namespace MoviesBroker.Services
{
    public class BingAsync : TaskService<BingObj>
    {
        private const string BINGKey = "BEBF1591DC156190D8BA4CB046FB771173332D94";
        private const string BINGRequestUrl =
            "http://api.bing.net/json.aspx?AppId={0}&Query={1}&Sources=Translation&Version=2.2&Translation.SourceLanguage={2}&Translation.TargetLanguage={3}";

        public BingAsync(string text, string language)
        {
            string bingRequestUri =
                    String.Format(
                        BINGRequestUrl,
                        BINGKey,
                        text,
                        "en",
                        language);
            WebRequest bingRequest = (HttpWebRequest)WebRequest.Create(bingRequestUri);

            _task = Task.Factory.FromAsync(bingRequest.BeginGetResponse
                                                    ,
                                                    (Func<IAsyncResult, WebResponse>)
                                                    bingRequest.EndGetResponse
                                                    , null);   
        }

    }
}