using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using MoviesBroker.Models.ExternalModels;

namespace MoviesBroker.Services
{
    public class ImdbAsync: TaskService<IMDB>
    {
        public ImdbAsync(string title, int? year)
        {
            string imdbRequestUri = String.Format("http://imdbapi.com/?t={0}&y={1}", title, year);

            SetCachedResponseTask(imdbRequestUri);
        }
    }
}