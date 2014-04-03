using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieFetcher
{
    public class EntgroupPiaofang
    {
        public DateTime pStart = new DateTime();
        public DateTime pEnd = new DateTime();
        public int weekBox = 0;
        public int totalBox = 0;
        public string unit = "";
        public EntgroupPiaofang()
        { }
        public EntgroupPiaofang(string startdate, string enddate, string weekdata, string totaldata, string u)
        {
            DateTime.TryParse(startdate,out pStart);
            DateTime.TryParse(enddate, out pEnd);
            int.TryParse(weekdata.Replace(",",""), out weekBox);
            int.TryParse(totaldata.Replace(",", ""), out totalBox);
            unit = u;
        }
    }
}
