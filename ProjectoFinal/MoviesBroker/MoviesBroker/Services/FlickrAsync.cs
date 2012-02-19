using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using MoviesBroker.Models.ExternalModels;

namespace MoviesBroker.Services
{
    public class FlickrAsync : TaskService<FlickrSearchResponse>
    {
        private const string FlickrKey = "d4ad0d44e856a9bd626e93b841eb46b4";
        private const string FlickrSecret = "dd3d60b64d72bdf8";

        private const string FlickrRequestUrl =
            "http://api.flickr.com/services/rest/?method=flickr.photos.search&api_key={0}&format=json&nojsoncallback=1&text={1}+{2}&sort=interestingness-desc";

        public FlickrAsync(string title, string director)
        {
            string flickrRequestUri =
                string.Format(
                    FlickrRequestUrl,
                    FlickrKey,
                    title,
                    director
                    );

            WebRequest flickrRequest = (HttpWebRequest)WebRequest.Create(flickrRequestUri);
            _task = Task.Factory.FromAsync(flickrRequest.BeginGetResponse,
                                                    (Func<IAsyncResult, WebResponse>)flickrRequest.EndGetResponse,
                                                    null);
        }
    }
}