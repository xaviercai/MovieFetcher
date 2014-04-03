using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;

namespace MovieFetcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            byte[] mtimeHome = client.DownloadData("http://www.mtime.com");
            string strMtimeHome = System.Text.Encoding.UTF8.GetString(mtimeHome);
            int hotplayMovieSlideStart = 0;
            hotplayMovieSlideStart = strMtimeHome.IndexOf("hotplayMovieSlide");
            hotplayMovieSlideStart = strMtimeHome.IndexOf("<", hotplayMovieSlideStart);
            int hotplayMovieSlideEnd = hotplayMovieSlideStart + 1;
            hotplayMovieSlideEnd = strMtimeHome.IndexOf("</dl>", hotplayMovieSlideStart);
            string strHotplayMovieSlide = strMtimeHome.Substring(hotplayMovieSlideStart, hotplayMovieSlideEnd - hotplayMovieSlideStart);

            int upcomingSlideStart = 0;
            upcomingSlideStart = strMtimeHome.IndexOf("upcomingSlide");
            upcomingSlideStart = strMtimeHome.IndexOf("<", upcomingSlideStart);
            int upcomingSlideEnd = upcomingSlideStart + 1;
            upcomingSlideEnd = strMtimeHome.IndexOf("</dl>", upcomingSlideStart);
            string strUpcomingSlide = strMtimeHome.Substring(upcomingSlideStart, upcomingSlideEnd - upcomingSlideStart);

            List<string> hotMovieList = new List<string>();
            int tmp1,tmp2;
            while (strHotplayMovieSlide.IndexOf("movieid") >= 0)
            {
                tmp1 = strHotplayMovieSlide.IndexOf("movieid")+9;
                tmp2 = strHotplayMovieSlide.IndexOf(">",tmp1)-1;
                hotMovieList.Add(strHotplayMovieSlide.Substring(tmp1, tmp2 - tmp1));
                strHotplayMovieSlide = strHotplayMovieSlide.Substring(strHotplayMovieSlide.IndexOf("</dd>")+4);
            }

            List<string> upcomingList = new List<string>();
            while (strUpcomingSlide.IndexOf("movieid") >= 0)
            {
                tmp1 = strUpcomingSlide.IndexOf("movieid") + 9;
                tmp2 = strUpcomingSlide.IndexOf(">", tmp1) - 1;
                upcomingList.Add(strUpcomingSlide.Substring(tmp1, tmp2 - tmp1));
                strUpcomingSlide = strUpcomingSlide.Substring(strUpcomingSlide.IndexOf("</dd>") + 4);
            }
            //for hotplay movies

            foreach (string strId in hotMovieList)
            {
                byte[] movieDetail = client.DownloadData("");
            }


            //for upcoming movies

        }
    }
}
