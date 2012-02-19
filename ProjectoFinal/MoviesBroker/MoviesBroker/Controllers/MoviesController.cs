using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Async;
using MoviesBroker.Models;
using MoviesBroker.Models.ExternalModels;
using MoviesBroker.Services;

namespace MoviesBroker.Controllers
{
    public class MoviesController : AsyncController
    {
        public void IndexAsync(){ }
        public string IndexCompleted()
        {
            return "use /info?t={title}[&y={year}][&l={language}]";
        }


        //GET t (movie), y (year), l (language) 
        public void InfoAsync(string t, int? y, string l="en")
        {
            var service = new MovieInfoAsync(AsyncManager);
            service.GetMovieInfo(t, y, l).Run();
        }

        public JsonResult InfoCompleted(MovieResponse movie)
        {
            if (movie.status_code != HttpStatusCode.OK)
            {
                Response.StatusCode = (int) movie.status_code;
                return Json(new MovieResponse() {status_code = movie.status_code, message = movie.message});
            }

            return Json(movie, JsonRequestBehavior.AllowGet);
        }



    }
}
