using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using MoviesBroker.Models.ExternalModels;

namespace MoviesBroker.Services
{
    public class NYTAsync : TaskService<NYTASearchResponse>
    {
        private const string NYTKey = "608e2aac74f920de32345bdcbd920b27:10:65684932";
        private const string NYTRequestUrl =
            "http://api.nytimes.com/svc/movies/v2/reviews/search.json?query={0}&api-key={1}&opening-date={2}-01-01;{3}-12-31";

        public NYTAsync(string title, int year)
        {
            string nytRequestUri =
                string.Format(
                    NYTRequestUrl
                    , title
                    , NYTKey
                    , year
                    , DateTime.Now.Year //imdbObj.Year + 1
                    );

            SetCachedResponseTask(nytRequestUri);
        }
    }
}