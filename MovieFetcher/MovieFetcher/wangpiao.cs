using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieFetcher
{
    public class wangpiao
    {
        public string zongchangci = "";
        public string tongjichangci = "";
        public string changjun = "";
        public string shangzuolv = "";
        public string renci = "";
        public string piaojia = "";
        public string piaofang = "";
        public string moviename = "";
        public string movieid = "";
        public wangpiao()
        {
        }
        public wangpiao(string zc,string tc,string cj,string sz,string rc,string pj,string pf,string mn,string mid)
        {
            zongchangci = zc;
            tongjichangci = tc;
            changjun = cj;
            shangzuolv = sz;
            renci = rc;
            piaojia = pj;
            piaofang = pf;
            moviename = mn;
            movieid = mid;
        }
    }
}
