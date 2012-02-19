using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MoviesBroker.Models
{
    public class MovieResponse
    {
        public string title { get; set; }
        public int? year { get; set; }
        public string language { get; set; }

        public HttpStatusCode status_code { get; set; }
        public string message { get; set; }

        public Movie movie { get; set; }
    }
}