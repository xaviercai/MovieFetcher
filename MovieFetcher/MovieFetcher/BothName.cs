using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieFetcher
{
    public class BothName
    {
        string _enName;
        string _cnName;
        public string EnglishName
        {
            get { return _enName; }
            set { _enName = value.Replace("&#183;", "·").Replace("&amp;", "&"); }
        }

        public string ChineseName 
        {
            get { return _cnName; }
            set { _cnName = value.Replace("&#183;","·").Replace("&amp;","&"); }
        }
        public BothName(string enName, string cnName)
        {
            _enName = enName.Replace("&#183;", "·").Replace("&amp;", "&");
            _cnName = cnName.Replace("&#183;", "·").Replace("&amp;", "&");
        }
        public BothName() : this("", "") { }
    }
}
