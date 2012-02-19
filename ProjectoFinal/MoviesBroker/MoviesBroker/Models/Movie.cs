using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoviesBroker.Models
{
    public class Movie
    {
        public string title { get; set; }
        public int year { get; set; }
        public string director { get; set; }
        public string synopsis { get; set; }
        public Uri poster{ get; set; }
        public Uri[] photos { get; set; }
        public Critic[] critics { get; set; }



    }
}