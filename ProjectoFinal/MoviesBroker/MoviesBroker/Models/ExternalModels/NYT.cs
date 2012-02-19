using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoviesBroker.Models.ExternalModels
{
    public class NYTASearchResponse
    {
        public string Status { get; set; }
        public string Copyright { get; set; }
        public int Num_Results { get; set; }
        public NYTArticle[] Results { get; set; }
    }

    public class NYTLink
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string Suggested_Link_Text { get; set; }
    }

    public class NYTArticle
    {
        public int Id { get; set; }
        public string Display_Title { get; set; }
        public string Sort_Name { get; set; }
        public string Mpaa_Rating { get; set; }
        public string Critics_Pick { get; set; }
        public string Thousand_Best { get; set; }
        public string Byline { get; set; }
        public string Headline { get; set; }
        public string Capsule_Review { get; set; }
        public string Summary_Short { get; set; }
        public string Publication_Date{ get; set; }
        public string Opening_Date { get; set; }
        public string Dvd_Release_Date { get; set; }
        public string Data_Updated { get; set; }
        public string Seo_Name { get; set; }
        public NYTLink Link { get; set; }
        public NYTLink[] Related_Urls { get; set; }
    }
}