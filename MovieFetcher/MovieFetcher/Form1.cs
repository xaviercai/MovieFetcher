using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;


namespace MovieFetcher
{
    
    public partial class Form1 : Form
    {
        private string mtimeHomePage = "";
        Dictionary<string, bool> docCompleted = new Dictionary<string, bool>();
        Dictionary<string, List<string>> docs = new Dictionary<string, List<string>>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
        #region entgroup
        private string getEntgroupJson()
        {
            WebClient client = new WebClient();
            string postData = "{}";
            HttpWebRequest requestJSON = (HttpWebRequest)WebRequest.Create("http://data.entgroup.cn/BoxOffice/information/file_qb.aspx/GetNowList");
            byte[] data = System.Text.Encoding.UTF8.GetBytes(postData);
            requestJSON.Method = "Post";
            requestJSON.ContentType = "application/json; charset=utf-8";
            requestJSON.ContentLength = data.Length; requestJSON.KeepAlive = true;
            System.IO.Stream stream = requestJSON.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            HttpWebResponse responseJson = (HttpWebResponse)requestJSON.GetResponse();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseJson.GetResponseStream(), System.Text.Encoding.UTF8);
            string content = reader.ReadToEnd();
            responseJson.Close();
            requestJSON = null;
            responseJson = null; reader = null;
            stream = null;
            return content;
        }
        private List<string> getEntgroupMovieList(string entgroupJson)
        {
            List<string> movies = new List<string>();

            int start = 0;
            int end = 0;
            while (entgroupJson.IndexOf("http://m.entgroup.cn/", start) > 0)
            {
                start = entgroupJson.IndexOf("http://m.entgroup.cn/", start)+21;
                end = entgroupJson.IndexOf("/", start);
                movies.Add(entgroupJson.Substring(start, end - start));
            }

            return movies;
        }
        private string GetEntgroupMovieHome(string movieid)
        {
            WebClient client = new WebClient();
            byte[] movieHome=client.DownloadData("http://m.entgroup.cn/" + movieid + "/");
            return System.Text.Encoding.UTF8.GetString(movieHome);
        }
        private BothName getMovieName(string strMovieHome)
        {
            string enName = "";
            string cnName = "";
            int start = 0, end = 0;
            if (strMovieHome.IndexOf("lblcnName") > 0)
            {
                start = strMovieHome.IndexOf("lblcnName");
                end = strMovieHome.IndexOf("</h1>", start);
                start = strMovieHome.LastIndexOf(">", end) + 1;
                cnName = strMovieHome.Substring(start, end - start);
            }
            if (strMovieHome.IndexOf("lblenName") > 0)
            {
                start = strMovieHome.IndexOf("lblenName");
                end = strMovieHome.IndexOf("</h2>", start);
                start = strMovieHome.LastIndexOf(">", end) + 1;
                enName = strMovieHome.Substring(start, end - start);

            }

            return new BothName(enName, cnName);
        }
        /// <summary>
        /// datetime min value means no data collected
        /// </summary>
        /// <param name="strMovieHome"></param>
        /// <returns></returns>
        private DateTime getReleaseDate(string strMovieHome)
        {
            if (strMovieHome.IndexOf("lblFilmingDates") > 0)
            {
                int start = strMovieHome.IndexOf("lblFilmingDates");
                start = strMovieHome.IndexOf(">", start) + 1;

                int end = strMovieHome.IndexOf("年",start);
                string syear = "", smonth = "", sday = "";
                syear = strMovieHome.Substring(start, end-start);
                start = end + 1;
                end = strMovieHome.IndexOf("月", start);
                smonth = strMovieHome.Substring(start, end - start);
                start = end + 1;
                end = strMovieHome.IndexOf("日", start);
                sday = strMovieHome.Substring(start, end - start);
                int year = 0, month = 0, day = 0;
                if (int.TryParse(syear, out year) && int.TryParse(smonth, out month) && int.TryParse(sday, out day))
                {
                    return new DateTime(year, month, day);
                }
            }
            return DateTime.MinValue;
        }
        private List<EntgroupPiaofang> getEntgroupPiaofang(string strMovieHome)
        {
            List<EntgroupPiaofang> pList = new List<EntgroupPiaofang>();
            int start = 0;
            int end = 0;
            string strStart = "";
            string strEnd = "";
            string weekdata = "";
            string totaldata = "";
            if (strMovieHome.IndexOf("pnl1")>0)
            {
                start = strMovieHome.IndexOf("pnl1");
                start = strMovieHome.IndexOf("</tr>", start)+5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                while (strMovieHome.IndexOf("</tr>", start) < strMovieHome.IndexOf("</table",start) && strMovieHome.IndexOf("</tr>", start)>0)
                {
                    start = strMovieHome.IndexOf("<td>", start)+4;
                    end = strMovieHome.IndexOf("至", start);
                    strStart=strMovieHome.Substring(start,end-start).Trim();
                    start=end+1;
                    end=strMovieHome.IndexOf("<strong>",start);
                    strEnd=strMovieHome.Substring(start,end-start).Trim();
                    start = strMovieHome.IndexOf("</span>", start)+7;
                    end = strMovieHome.IndexOf("</td>", start);
                    weekdata = strMovieHome.Substring(start, end - start);
                    start = strMovieHome.IndexOf("</span>", end) + 7;
                    end = strMovieHome.IndexOf("</td>", start);
                    totaldata = strMovieHome.Substring(start, end - start);
                    pList.Add(new EntgroupPiaofang(strStart, strEnd, weekdata, totaldata, "RMB"));
                    start = strMovieHome.IndexOf("</tr>", start) + 5;
                }
            }
            if (strMovieHome.IndexOf("pnl2") > 0)
            {
                start = strMovieHome.IndexOf("pnl2");
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                while (strMovieHome.IndexOf("</tr>", start) < strMovieHome.IndexOf("</table",start) && strMovieHome.IndexOf("</tr>", start) > 0)
                {
                    start = strMovieHome.IndexOf("<td>", start) + 4;
                    end = strMovieHome.IndexOf("至", start);
                    strStart = strMovieHome.Substring(start, end - start).Trim();
                    start = end + 1;
                    end = strMovieHome.IndexOf("<strong>", start);
                    strEnd = strMovieHome.Substring(start, end - start).Trim();
                    start = strMovieHome.IndexOf("</span>", start) + 7;
                    end = strMovieHome.IndexOf("</td>", start);
                    weekdata = strMovieHome.Substring(start, end - start);
                    start = strMovieHome.IndexOf("</span>", end) + 7;
                    end = strMovieHome.IndexOf("</td>", start);
                    totaldata = strMovieHome.Substring(start, end - start);
                    pList.Add(new EntgroupPiaofang(strStart, strEnd, weekdata, totaldata, "HKD"));
                    start = strMovieHome.IndexOf("</tr>", start) + 5;
                }
            }
            if (strMovieHome.IndexOf("pnl3") > 0)
            {
                start = strMovieHome.IndexOf("pnl3");
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                start = strMovieHome.IndexOf("</tr>", start) + 5;
                while (strMovieHome.IndexOf("</tr>", start) < strMovieHome.IndexOf("</table",start) && strMovieHome.IndexOf("</tr>", start) > 0)
                {
                    start = strMovieHome.IndexOf("<td>", start) + 4;
                    end = strMovieHome.IndexOf("至", start);
                    strStart = strMovieHome.Substring(start, end - start).Trim();
                    start = end + 1;
                    end = strMovieHome.IndexOf("<strong>", start);
                    strEnd = strMovieHome.Substring(start, end - start).Trim();
                    start = strMovieHome.IndexOf("</span>", start) + 7;
                    end = strMovieHome.IndexOf("</td>", start);
                    weekdata = strMovieHome.Substring(start, end - start);
                    start = strMovieHome.IndexOf("</span>", end) + 7;
                    end = strMovieHome.IndexOf("</td>", start);
                    totaldata = strMovieHome.Substring(start, end - start);
                    pList.Add(new EntgroupPiaofang(strStart, strEnd, weekdata, totaldata, "USD"));
                    start = strMovieHome.IndexOf("</tr>", start) + 5;
                }
            }

            return pList;
        }

        private void recordEntgroupMovieInfo()
        {
            string eJson = getEntgroupJson();
            List<string> movies = getEntgroupMovieList(eJson);
            string moviehome = "";
            using (DataBase db = new DataBase())
            {
                string insertSql = "INSERT INTO [ENTGROUP_MOVIE_INFO]([MOVIE_ID],[MOVIE_NAME_CN],[MOVIE_NAME_EN],[RELEASE_DATE] "+
",[START_DATE],[END_DATE],[WEEKLY_BOX],[TOTAL_BOX],[UNIT],[RECORD_TIME]) VALUES (@MOVIE_ID "+
",@MOVIE_NAME_CN,@MOVIE_NAME_EN,@RELEASE_DATE,@START_DATE,@END_DATE,@WEEKLY_BOX,@TOTAL_BOX,@UNIT,@RECORD_TIME)";
                foreach (string movieid in movies)
                {
                    moviehome = GetMovieHome(movieid);
                    BothName movieName = getMovieName(moviehome);
                    DateTime releaseDate = getReleaseDate(moviehome);
                    List<EntgroupPiaofang> pList = getEntgroupPiaofang(moviehome);
                    foreach (EntgroupPiaofang pInfo in pList)
                    {
                        if (CheckIfExistEntgroup(pInfo.pStart, pInfo.pEnd, movieid)) continue;
                        db.AddParameter("MOVIE_ID", movieid);
                        db.AddParameter("MOVIE_NAME_CN", movieName.ChineseName);
                        db.AddParameter("MOVIE_NAME_EN", movieName.EnglishName);
                        db.AddParameter("RELEASE_DATE", releaseDate);
                        db.AddParameter("START_DATE", pInfo.pStart);
                        db.AddParameter("END_DATE", pInfo.pEnd);
                        db.AddParameter("WEEKLY_BOX", pInfo.weekBox);
                        db.AddParameter("TOTAL_BOX", pInfo.totalBox);
                        db.AddParameter("UNIT", pInfo.unit);
                        db.AddParameter("RECORD_TIME", DateTime.Now);
                        db.ExecuteNonQuery(insertSql);
                    }
                }
            }
        }
        private bool CheckIfExistEntgroup(DateTime startdate, DateTime enddate, string movieid)
        {
            string sql = "SELECT ID FROM ENTGROUP_MOVIE_INFO WHERE MOVIE_ID=@MOVIE_ID AND START_DATE=@START_DATE AND END_DATE=@END_DATE";
            using (DataBase db = new DataBase())
            {
                db.AddParameter("MOVIE_ID", movieid);
                db.AddParameter("START_DATE", startdate);
                db.AddParameter("END_DATE", enddate);
                DataTable dt = db.ExecuteDataTable(sql);
                if (dt.Rows.Count > 0) return true;
            }
            return false;
        }
        #endregion

        private void recordMtimeInfo()
        {
            WebClient client = new WebClient();
            byte[] mtimeHome = client.DownloadData("http://www.mtime.com");
            string strMtimeHome = System.Text.Encoding.UTF8.GetString(mtimeHome);
            mtimeHomePage = strMtimeHome;
            
            List<string> hotList = GetHotMovieList(strMtimeHome);
            List<string> upcomingList = GetUpcomingList(strMtimeHome);
            using (DataBase db = new DataBase())
            {
                string insertBaseInfoSql = "INSERT INTO [MTIME_MOVIE_INFO]([MOVIE_ID],[CN_NAME],[EN_NAME],[DISTRICTS],[DIRECTOR],[DIRECTOR_EN] " +
",[ACTORS],[COMPANYS],[RECORD_TIME]) VALUES (@MOVIE_ID,@CN_NAME,@EN_NAME,@DISTRICTS "+
",@DIRECTOR,@ACTORS,@COMPANYS,@RECORD_TIME)";
                string insertStatusInfoSql = "INSERT INTO [MTIME_MOVIE_STATUS]([MOVIE_ID],[RATE],[WANT],[TOTAL_BOX],[STATUS] "+
",[RECORD_TIME],[LAST_UPDATE_TIME]) VALUES (@MOVIE_ID,@RATE,@WANT,@TOTAL_BOX "+
",@STATUS,@RECORD_TIME,@LAST_UPDATE_TIME)";
                foreach (string movieid in hotList)
                {
                    int mid = getMovieIDFromDB(movieid);
                    string strMovieHome = GetMovieHome(movieid);
                    string strMovieDetail = GetMoviefullcredits(movieid);
                    string strOverallInfo = GetMovieOverallInfo(movieid);
                    if (mid == 0)
                    {
                        //insert into movie info table
                        db.AddParameter("MOVIE_ID", movieid);
                        BothName moviename = GetMovieName(strMovieHome);
                        db.AddParameter("CN_NAME", moviename.ChineseName);
                        db.AddParameter("EN_NAME", moviename.EnglishName);
                        List<string> districts = GetDistrict(movieid);
                        string strDistricts = "";
                        foreach (string d in districts)
                        {
                            strDistricts += d + ";";
                        }
                        db.AddParameter("DISTRICTS", strDistricts);

                        BothName director = GetDirector(strMovieDetail);
                        db.AddParameter("DIRECTOR", director.ChineseName);
                        db.AddParameter("DIRECTOR_EN", director.EnglishName);
                        List<BothName> actorList = GetActor(strMovieDetail);
                        string actors = "";
                        foreach (BothName a in actorList)
                        {
                            actors += a.ChineseName + "(" + a.EnglishName + ");";
                        }
                        db.AddParameter("ACTORS", actors);
                        List<string> companyList = GetCompany(movieid);
                        string companys = "";
                        foreach (string c in companyList)
                        {
                            companys += c + ";";
                        }
                        companyList.Clear();
                        companyList = GetCompany2(movieid);
                        foreach (string c in companyList)
                        {
                            companys += c + ";";
                        }
                        db.AddParameter("COMPANYS", companys);
                        db.AddParameter("RECORD_TIME", DateTime.Now);
                        db.ExecuteNonQuery(insertBaseInfoSql);
                    }
                    //update status
                    db.AddParameter("MOVIE_ID", movieid);
                    db.AddParameter("RATE", GetRate(strOverallInfo));
                    db.AddParameter("WANT", GetWant(strOverallInfo));
                    db.AddParameter("TOTAL_BOX", GetTotalBox(strOverallInfo));
                    db.AddParameter("STATUS", "HOT");
                    db.AddParameter("RECORD_TIME", DateTime.Now);
                    db.AddParameter("LAST_UPDATE_TIME", DateTime.Now);
                    db.ExecuteNonQuery(insertStatusInfoSql);
                }
                foreach (string movieid in upcomingList)
                {
                    int mid = getMovieIDFromDB(movieid);
                    string strMovieHome = GetMovieHome(movieid);
                    string strMovieDetail = GetMoviefullcredits(movieid);
                    string strOverallInfo = GetMovieOverallInfo(movieid);
                    if (mid == 0)
                    {
                        //insert into movie info table
                        db.AddParameter("MOVIE_ID", movieid);
                        BothName moviename = GetMovieName(strMovieHome);
                        db.AddParameter("CN_NAME", moviename.ChineseName);
                        db.AddParameter("EN_NAME", moviename.EnglishName);
                        List<string> districts = GetDistrict(movieid);
                        string strDistricts = "";
                        foreach (string d in districts)
                        {
                            strDistricts += d + ";";
                        }
                        db.AddParameter("DISTRICTS", strDistricts);

                        BothName director = GetDirector(strMovieDetail);
                        db.AddParameter("DIRECTOR", director.ChineseName);
                        db.AddParameter("DIRECTOR_EN", director.EnglishName);
                        List<BothName> actorList = GetActor(strMovieDetail);
                        string actors = "";
                        foreach (BothName a in actorList)
                        {
                            actors += a.ChineseName + "(" + a.EnglishName + ");";
                        }
                        db.AddParameter("ACTORS", actors);
                        List<string> companyList = GetCompany(movieid);
                        string companys = "";
                        foreach (string c in companyList)
                        {
                            companys += c + ";";
                        }
                        companyList.Clear();
                        companyList = GetCompany2(movieid);
                        foreach (string c in companyList)
                        {
                            companys += c + ";";
                        }
                        db.AddParameter("COMPANYS", companys);
                        db.AddParameter("RECORD_TIME", DateTime.Now);
                        db.ExecuteNonQuery(insertBaseInfoSql);
                    }
                    //update status
                    db.AddParameter("MOVIE_ID", movieid);
                    db.AddParameter("RATE", GetRate(strOverallInfo));
                    db.AddParameter("WANT", GetWant(strOverallInfo));
                    db.AddParameter("TOTAL_BOX", GetTotalBox(strOverallInfo));
                    db.AddParameter("STATUS", "UPCOMING");
                    db.AddParameter("RECORD_TIME", DateTime.Now);
                    db.AddParameter("LAST_UPDATE_TIME", DateTime.Now);
                    db.ExecuteNonQuery(insertStatusInfoSql);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="movieid">Movie ID in mtime</param>
        /// <returns>ID in database</returns>
        private int getMovieIDFromDB(string movieid)
        {
            using(DataBase db=new DataBase())
            {
                DataTable dt=db.ExecuteDataTable("SELECT [ID] FROM [MTIME_MOVIE_INFO] WHERE [MOVIE_ID]='"+movieid+"'");
                if(dt.Rows.Count==1)
                {
                    return Convert.ToInt32(dt.Rows[0][0].ToString());
                }
            }
            return 0;
        }
        private void CollectData()
        {
            WebClient client = new WebClient();
            
            byte[] mtimeHome = client.DownloadData("http://www.mtime.com");
            string strMtimeHome = System.Text.Encoding.UTF8.GetString(mtimeHome);
            mtimeHomePage = strMtimeHome;
            textBox_viewer.AppendText("Hot Movies:\r\n");
            List<string> hotList = GetHotMovieList(strMtimeHome);
            foreach (string movieID in hotList)
            {
                textBox_viewer.AppendText("ID:\t" + movieID + "\r\n");
                BothName movieName = GetMovieName(GetMovieHome(movieID));
                textBox_viewer.AppendText("ChineseName:\t" + movieName.ChineseName + "\r\n");
                textBox_viewer.AppendText("EnglishName:\t" + movieName.EnglishName + "\r\n");

                List<string> districts = GetDistrict(movieID);
                textBox_viewer.AppendText("Districts:\t");
                foreach (string district in districts)
                {
                    textBox_viewer.AppendText(district + ";");
                }
                textBox_viewer.AppendText("\r\n");


                string overalInfo = GetMovieOverallInfo(movieID);
                textBox_viewer.AppendText("Rate:\t" + GetRate(overalInfo) + "\r\n");
                textBox_viewer.AppendText("Want:\t" + GetWant(overalInfo) + "\r\n");
                textBox_viewer.AppendText("TotalBox\t" + GetTotalBox(overalInfo) + "\r\n");


                string movieDetails = GetMoviefullcredits(movieID);
                BothName movieDirector = GetDirector(movieDetails);
                textBox_viewer.AppendText("Director:\t" + movieDirector.ChineseName + "(" + movieDirector.EnglishName + ")" + "\r\n");
                List<BothName> actorList = GetActor(movieDetails);
                textBox_viewer.AppendText("ActorList:" + "\r\n");
                foreach (BothName bname in actorList)
                {
                    textBox_viewer.AppendText("\t" + bname.ChineseName + "(" + bname.EnglishName + ")" + "\r\n");
                }
                List<string> companys2 = GetCompany2(movieID);
                textBox_viewer.AppendText("Manufacturing Companys:" + "\r\n");
                foreach (string company in companys2)
                {
                    textBox_viewer.AppendText("\t" + company + "\r\n");
                }

                List<string> companys = GetCompany(movieID);
                textBox_viewer.AppendText("Publisher Companys:" + "\r\n");
                foreach (string company in companys)
                {
                    textBox_viewer.AppendText("\t" + company + "\r\n");
                }
                textBox_viewer.AppendText("------------------------------------------------------------------" + "\r\n");


            }

            textBox_viewer.AppendText("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||" + "\r\n");
            textBox_viewer.AppendText("Upcoming Movies" + "\r\n");
            List<string> upcomingList = GetUpcomingList(strMtimeHome);
            foreach (string movieID in upcomingList)
            {
                textBox_viewer.AppendText("ID:\t" + movieID + "\r\n");
                BothName movieName = GetMovieName(GetMovieHome(movieID));
                textBox_viewer.AppendText("ChineseName:\t" + movieName.ChineseName + "\r\n");
                textBox_viewer.AppendText("EnglishName:\t" + movieName.EnglishName + "\r\n");

                List<string> districts = GetDistrict(movieID);
                textBox_viewer.AppendText("Districts:\t");
                foreach (string district in districts)
                {
                    textBox_viewer.AppendText(district + ";");
                }
                textBox_viewer.AppendText("\r\n");

                string overalInfo = GetMovieOverallInfo(movieID);
                textBox_viewer.AppendText("Rate:\t" + GetRate(overalInfo) + "\r\n");
                textBox_viewer.AppendText("Want:\t" + GetWant(overalInfo) + "\r\n");
                //textBox_viewer.AppendText("TotalBox\t" + GetTotalBox(overalInfo));


                string movieDetails = GetMoviefullcredits(movieID);
                BothName movieDirector = GetDirector(movieDetails);
                textBox_viewer.AppendText("Director:\t" + movieDirector.ChineseName + "(" + movieDirector.EnglishName + ")" + "\r\n");
                List<BothName> actorList = GetActor(movieDetails);
                textBox_viewer.AppendText("ActorList:" + "\r\n");
                foreach (BothName bname in actorList)
                {
                    textBox_viewer.AppendText("\t" + bname.ChineseName + "(" + bname.EnglishName + ")" + "\r\n");
                }

                List<string> companys2 = GetCompany2(movieID);
                textBox_viewer.AppendText("Manufacturing Companys:" + "\r\n");
                foreach (string company in companys2)
                {
                    textBox_viewer.AppendText("\t" + company + "\r\n");
                }

                List<string> companys = GetCompany(movieID);
                textBox_viewer.AppendText("Publisher Companys:" + "\r\n");
                foreach (string company in companys)
                {
                    textBox_viewer.AppendText("\t" + company + "\r\n");
                }
                textBox_viewer.AppendText("------------------------------------------------------------------" + "\r\n");
            }
            //GetBaiduInfo(strMtimeHome);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMtimeHome">mtime.com homepage</param>
        /// <returns></returns>
        private List<string> GetHotMovieList(string strMtimeHome)
        {
            int hotplayMovieSlideStart = 0;
            hotplayMovieSlideStart = strMtimeHome.IndexOf("hotplayMovieSlide");
            hotplayMovieSlideStart = strMtimeHome.IndexOf("<", hotplayMovieSlideStart);
            int hotplayMovieSlideEnd = hotplayMovieSlideStart + 1;
            hotplayMovieSlideEnd = strMtimeHome.IndexOf("</dl>", hotplayMovieSlideStart);
            string strHotplayMovieSlide = strMtimeHome.Substring(hotplayMovieSlideStart, hotplayMovieSlideEnd - hotplayMovieSlideStart);
            List<string> hotMovieList = new List<string>();
            int tmp1, tmp2;
            while (strHotplayMovieSlide.IndexOf("movieid") >= 0)
            {
                tmp1 = strHotplayMovieSlide.IndexOf("movieid") + 9;
                tmp2 = strHotplayMovieSlide.IndexOf(">", tmp1) - 1;
                hotMovieList.Add(strHotplayMovieSlide.Substring(tmp1, tmp2 - tmp1));
                strHotplayMovieSlide = strHotplayMovieSlide.Substring(strHotplayMovieSlide.IndexOf("</dd>") + 4);
            }
            return hotMovieList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMtimeHome">mtime.com homepage</param>
        /// <returns></returns>
        private List<string> GetUpcomingList(string strMtimeHome)
        {

            int upcomingSlideStart = 0;
            upcomingSlideStart = strMtimeHome.IndexOf("upcomingSlide");
            upcomingSlideStart = strMtimeHome.IndexOf("<", upcomingSlideStart);
            int upcomingSlideEnd = upcomingSlideStart + 1;
            upcomingSlideEnd = strMtimeHome.IndexOf("</dl>", upcomingSlideStart);
            string strUpcomingSlide = strMtimeHome.Substring(upcomingSlideStart, upcomingSlideEnd - upcomingSlideStart);
            List<string> upcomingList = new List<string>();
            int tmp1, tmp2;
            while (strUpcomingSlide.IndexOf("movieid") >= 0)
            {
                tmp1 = strUpcomingSlide.IndexOf("movieid") + 9;
                tmp2 = strUpcomingSlide.IndexOf(">", tmp1) - 1;
                upcomingList.Add(strUpcomingSlide.Substring(tmp1, tmp2 - tmp1));
                strUpcomingSlide = strUpcomingSlide.Substring(strUpcomingSlide.IndexOf("</dd>") + 4);
            }
            return upcomingList;
        }


        private string GetMoviefullcredits(string movieID)
        {
            WebClient client = new WebClient();
            byte[] movieDetail = client.DownloadData("http://movie.mtime.com/" + movieID + "/fullcredits.html");
            return System.Text.Encoding.UTF8.GetString(movieDetail);
        }

        private string GetMovieHome(string movieID)
        {
            WebClient client = new WebClient();
            byte[] movieDetail = client.DownloadData("http://movie.mtime.com/" + movieID + "/");
            return System.Text.Encoding.UTF8.GetString(movieDetail);
        }

        private string GetMovieOverallInfo(string movieID)
        {
            string url = "http://service.library.mtime.com/Movie.api?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Library.Services&Ajax_CallBackMethod=GetMovieOverviewRating&Ajax_CrossDomain=1&Ajax_CallBackArgument0=" + movieID;
            WebClient client = new WebClient();
            byte[] overalInfo= client.DownloadData(url);
            return System.Text.Encoding.UTF8.GetString(overalInfo);

        }


        private BothName GetDirector(string strMovieDetail)
        {
            int directorStart = strMovieDetail.IndexOf("Director");
            BothName directorName = new BothName();
            if (strMovieDetail.IndexOf("px16 normal") > 0)
            {
                directorStart = strMovieDetail.IndexOf("px16 normal");
            }
            int directorEnd = strMovieDetail.IndexOf("</a>", directorStart);
            directorStart = strMovieDetail.LastIndexOf(">", directorEnd)+1;
            
            directorName.ChineseName = strMovieDetail.Substring(directorStart, directorEnd - directorStart);
            directorName.ChineseName = directorName.ChineseName.Replace("&#183;", "·");
            //perhaps no english name
            int tmp1=0, tmp2=1;
            tmp1 = strMovieDetail.IndexOf("<p>", directorEnd);
            tmp2 = strMovieDetail.IndexOf("</div>", directorEnd);
            if (tmp2 < tmp1)
            {
                directorName.EnglishName = "";
                return directorName;
            }
            directorStart = strMovieDetail.IndexOf("_blank", directorEnd) + 8;
            directorEnd = strMovieDetail.IndexOf("</a>", directorStart);
            directorName.EnglishName = strMovieDetail.Substring(directorStart, directorEnd - directorStart);
            return directorName;
        }

        private List<BothName> GetActor(string strMovieDetail)
        {
            int actorStart = strMovieDetail.IndexOf("db_actor");
            int actorEnd = 0;
            string enName = "";
            string cnName = "";
            List<BothName> actorList = new List<BothName>();
            while (strMovieDetail.IndexOf("<dd>",actorStart) > 0)
            {
                actorStart = strMovieDetail.IndexOf("<dd>", actorStart);
                actorStart = strMovieDetail.IndexOf("<h3>", actorStart);
                //actorStart = strMovieDetail.IndexOf("_blank", actorStart)+8;
                actorEnd = strMovieDetail.IndexOf("</a>", actorStart);
                actorStart = strMovieDetail.LastIndexOf(">", actorEnd)+1;
                cnName = strMovieDetail.Substring(actorStart, actorEnd - actorStart);
                if (strMovieDetail.IndexOf("</div>", actorStart) < strMovieDetail.IndexOf("<p>", actorStart))
                    enName = "";
                else
                {
                    actorStart = strMovieDetail.IndexOf("<p>", actorStart);
                    actorEnd = strMovieDetail.IndexOf("</a>", actorStart);
                    actorStart = strMovieDetail.LastIndexOf(">", actorEnd) + 1;
                    enName = strMovieDetail.Substring(actorStart, actorEnd - actorStart);
                }
                actorList.Add(new BothName(enName, cnName));
                if (strMovieDetail.IndexOf("<dd>", actorEnd) > strMovieDetail.IndexOf("</dl>", actorEnd))
                    break;
                
            }
            return actorList;
        }

        private BothName GetMovieName(string strMovieHome)
        {
            int nameStart = strMovieHome.IndexOf("M14_Movie_Overview_Name");
            int nameEnd = strMovieHome.IndexOf("</h1>", nameStart);
            nameStart = strMovieHome.LastIndexOf(">", nameEnd)+1;
            string cnName = strMovieHome.Substring(nameStart, nameEnd - nameStart);

            if (strMovieHome.IndexOf("db_enname") < 0)
            {
                return new BothName("", cnName);
            }
            nameStart = strMovieHome.IndexOf("db_enname");
            nameEnd = strMovieHome.IndexOf("</p>", nameStart);
            nameStart = strMovieHome.LastIndexOf(">", nameEnd) + 1;
            string enName = strMovieHome.Substring(nameStart, nameEnd - nameStart);

            return new BothName(enName, cnName);
        }


        private string GetRate(string overallInfo)
        {
            int start = overallInfo.IndexOf("RatingFinal");
            start = overallInfo.IndexOf(":", start) + 1;
            int end = overallInfo.IndexOf(",", start);
            return overallInfo.Substring(start, end - start);
        }

        private string GetWant(string overallInfo)
        {
            int start = overallInfo.IndexOf("AttitudeCount");
            start = overallInfo.IndexOf(":", start) + 1;
            int end = overallInfo.IndexOf("}", start);
            return overallInfo.Substring(start, end - start);
        }

        private string GetTotalBox(string overallInfo)
        {
            int start = overallInfo.IndexOf("\"TotalBoxOffice\"");
            if (start < 0) return "NULL";
            start = overallInfo.IndexOf(":", start) + 1;
            int end = overallInfo.IndexOf(",", start);
            string totalBox = overallInfo.Substring(start, end - start).Trim('"');
            
            start = overallInfo.IndexOf("\"TotalBoxOfficeUnit\"");
            start = overallInfo.IndexOf(":", start) + 1;
            end = overallInfo.IndexOf(",", start);
            totalBox += overallInfo.Substring(start, end - start).Trim('"');
            return totalBox;
        }

        private List<string> GetCompany(string MovieID)
        {
            WebClient client = new WebClient();
            byte[] companyInfo = client.DownloadData("http://movie.mtime.com/" + MovieID + "/details.html");
            string strCompanyInfo = System.Text.Encoding.UTF8.GetString(companyInfo);
            List<string> companyList = new List<string>();

            int start = strCompanyInfo.IndexOf("companyRegion");
            start = strCompanyInfo.IndexOf("</div>", start);
            start = strCompanyInfo.IndexOf("<ul>", start);
            int end=0;
            while(start>0 && strCompanyInfo.IndexOf("<li",start)>0)
            {
                start=strCompanyInfo.IndexOf("<li",start);
                end = strCompanyInfo.IndexOf("</a>", start);
                start = strCompanyInfo.LastIndexOf(">", end)+1;
                companyList.Add(strCompanyInfo.Substring(start, end - start).Replace("&#183;", "·").Replace("&amp;", "&"));
                if (strCompanyInfo.IndexOf("</ul>", end)>0 && strCompanyInfo.IndexOf("<li", end) > strCompanyInfo.IndexOf("</ul>", end))
                    break;
                if (strCompanyInfo.IndexOf("showMoreCompany", end) > 0 && strCompanyInfo.IndexOf("<li", end) > strCompanyInfo.IndexOf("showMoreCompany", end))
                    break;
            }
            return companyList;
        }

        private List<string> GetCompany2(string MovieID)
        {
            WebClient client = new WebClient();
            byte[] companyInfo = client.DownloadData("http://movie.mtime.com/" + MovieID + "/details.html");
            string strCompanyInfo = System.Text.Encoding.UTF8.GetString(companyInfo);
            List<string> companyList = new List<string>();

            int start = strCompanyInfo.IndexOf("companyRegion");
            //start = strCompanyInfo.IndexOf("</div>", start);
            start = strCompanyInfo.IndexOf("<ul>", start);
            int end = 0;
            while (start > 0 && strCompanyInfo.IndexOf("<li", start) > 0)
            {
                start = strCompanyInfo.IndexOf("<li", start);
                end = strCompanyInfo.IndexOf("</a>", start);
                start = strCompanyInfo.LastIndexOf(">", end) + 1;
                companyList.Add(strCompanyInfo.Substring(start, end - start).Replace("&#183;", "·").Replace("&amp;", "&"));
                if (strCompanyInfo.IndexOf("</ul>", end) > 0 && strCompanyInfo.IndexOf("<li", end) > strCompanyInfo.IndexOf("</ul>", end))
                    break;
                if (strCompanyInfo.IndexOf("showMoreCompany", end) > 0 && strCompanyInfo.IndexOf("<li", end) > strCompanyInfo.IndexOf("showMoreCompany", end))
                    break;
            }
            return companyList;
        }

        private List<string> GetDistrict(string MovieID)
        {
            WebClient client = new WebClient();
            byte[] movieInfo = client.DownloadData("http://movie.mtime.com/" + MovieID + "/");
            string strMovieInfo = System.Text.Encoding.UTF8.GetString(movieInfo);

            
            int districtStart = strMovieInfo.IndexOf("nation");
            
            List<string> districts = new List<string>();
            while (districtStart > 0)
            {
                int districtEnd = strMovieInfo.IndexOf("</a>", districtStart);
                districtStart = strMovieInfo.LastIndexOf(">", districtEnd) + 1;


                districts.Add(strMovieInfo.Substring(districtStart, districtEnd - districtStart));
                districtStart = strMovieInfo.IndexOf("nation", districtEnd);
            }
            return districts;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CollectData();
            Thread t = new Thread(new ThreadStart(GetBaiduInfo));
            t.Start();
           
        }


        private List<string> GetBaiduIndexByContent(string content)
        {
            int start = content.IndexOf("tpWord");
            if (start < 0) return new List<string>();
            start = content.IndexOf("ftlwhf", start) + 7;
            int end = content.IndexOf("</", start);

            List<string> baiduIndex = new List<string>();
            baiduIndex.Add(content.Substring(start, end - start));
            start = content.IndexOf("ftlwhf", start) + 7;
            end = content.IndexOf("</", start);
            baiduIndex.Add(content.Substring(start, end - start));


            start = content.IndexOf("tpWord", start);
            if (start < 0) return new List<string>();
            start = content.IndexOf("ftlwhf", start) + 7;
            end = content.IndexOf("</", start);
            baiduIndex.Add(content.Substring(start, end - start));

            start = content.IndexOf("ftlwhf", start) + 7;
            end = content.IndexOf("</", start);
            baiduIndex.Add(content.Substring(start, end - start));



            return baiduIndex;

        }

        private List<string> GetBaiduIndex(string word)
        {
            string url = "http://index.baidu.com/?tpl=trend&word=" + System.Web.HttpUtility.UrlEncode(word,System.Text.Encoding.GetEncoding("GB2312"));


            
            webBrowser1.Navigate(url);
            if (docCompleted.Keys.Contains(url))
                docCompleted[url] = false;
            else
                docCompleted.Add(url, false);
            int cnt = 0;
            while (!docCompleted[url] && cnt < 10) { Thread.Sleep(5000); cnt++; }
            if (cnt == 10) return new List<string>();
            Thread.Sleep(5000);//wait 5 secs for the js to complete
            CrossThreadGetHTML crossGetHTML = delegate()
            {
                if (webBrowser1.Document.All["mainWrap"] == null) return;
                string content = webBrowser1.Document.All["mainWrap"].InnerHtml;
                int tmpstart = content.IndexOf("tpWord");
                tmpstart = content.IndexOf("tpWord", tmpstart);
                cnt = 0;
                while (tmpstart < 0 && cnt < 10)
                {
                    Thread.Sleep(5000);
                    content = webBrowser1.Document.All["mainWrap"].InnerHtml;
                    tmpstart = content.IndexOf("tpWord");
                    tmpstart = content.IndexOf("tpWord", tmpstart);
                    cnt++;

                }
                content = webBrowser1.Document.All["mainWrap"].InnerHtml;
                docs[url]= GetBaiduIndexByContent(content);
            };
            webBrowser1.Invoke(crossGetHTML);
            if (docs.Keys.Contains(url)) return docs[url];
            else return new List<string>();
            //string content = webBrowser1.Document.Body.InnerHtml;
            //int start = content.IndexOf("tpWord");
            //if (start < 0) return new List<string>();
            //start = content.IndexOf("ftlwhf", start)+7;
            //int end = content.IndexOf("</", start);

            //List<string> baiduIndex = new List<string>();
            //baiduIndex.Add(content.Substring(start, end - start));
            //start = content.IndexOf("ftlwhf", start) + 7;
            //end = content.IndexOf("</", start);
            //baiduIndex.Add(content.Substring(start, end - start));


            //start = content.IndexOf("tpWord",start);
            //if (start < 0) return new List<string>();
            //start = content.IndexOf("ftlwhf", start) + 7;
            //end = content.IndexOf("</", start);
            //baiduIndex.Add(content.Substring(start, end - start));

            //start = content.IndexOf("ftlwhf", start) + 7;
            //end = content.IndexOf("</", start);
            //baiduIndex.Add(content.Substring(start, end - start));



            //return baiduIndex;
        }



        private void recordBaiduIndex(string movieid)
        {
            int mid = getUpdateID(movieid);
            BothName movieName = GetMovieName(GetMovieHome(movieid));
            List<string> bdIndex = GetBaiduIndex(movieName.ChineseName);
            List<string> bdIndex2 = GetBaiduIndex(movieName.EnglishName);
            int sevenOverall = 0;
            int sevenMobile = 0;
            int thirtyOverall = 0;
            int thirtyMobile = 0;
            if (bdIndex.Count == 4)
            {
                sevenOverall=Convert.ToInt32(bdIndex[0].Replace(",",""));
                sevenMobile = Convert.ToInt32(bdIndex[1].Replace(",", ""));
                thirtyOverall=Convert.ToInt32(bdIndex[2].Replace(",", ""));
                thirtyMobile = Convert.ToInt32(bdIndex[3].Replace(",", ""));
            }
            if (bdIndex2.Count == 4)
            {
                sevenOverall += Convert.ToInt32(bdIndex[0].Replace(",", ""));
                sevenMobile += Convert.ToInt32(bdIndex[1].Replace(",", ""));
                thirtyOverall += Convert.ToInt32(bdIndex[2].Replace(",", ""));
                thirtyMobile += Convert.ToInt32(bdIndex[3].Replace(",", ""));
            }
            //insert into db
        }
        //update baidu id
        private int getUpdateID(string movieID)
        {
            using (DataBase db = new DataBase())
            {
                DataTable dt = db.ExecuteDataTable("SELECT ID FROM MTIME_MOVIE_STATUS WHERE MOVIE_ID='" + movieID + "' AND BAIDU_7_DAY_OVERALL IS NULL AND RECORD_TIME>'"+DateTime.Now.ToString("yyyy-MM-dd")+"'");
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0].ToString());
                }

            }
            return 0;
        }
        private void GetBaiduInfo()
        {
            string strMtimeHome = mtimeHomePage;
            List<string> hotMovies = GetHotMovieList(strMtimeHome);
            List<string> upcomingMovies = GetUpcomingList(strMtimeHome);
            textBox_viewer.AppendText("||||||||||||||||||||||||||||||||||||||||||||||||||\r\n");
            foreach (string movieID in hotMovies)
            {
                textBox_viewer.AppendText("ID:\t" + movieID+"\r\n");
                BothName movieName = GetMovieName(GetMovieHome(movieID));
                textBox_viewer.AppendText("Chinese Name:\t" + movieName.ChineseName + "\r\n");
                List<string> bdindex = GetBaiduIndex(movieName.ChineseName);
                textBox_viewer.AppendText("Baidu Index:\r\n");
                if (bdindex.Count == 4)
                {
                    textBox_viewer.AppendText("最近7天整体搜索指数:\t" + bdindex[0] + "\r\n");
                    textBox_viewer.AppendText("最近7天移动搜索指数:\t" + bdindex[1] + "\r\n");
                    textBox_viewer.AppendText("最近30天整体搜索指数:\t" + bdindex[2] + "\r\n");
                    textBox_viewer.AppendText("最近30天移动搜索指数:\t" + bdindex[3] + "\r\n");
                }
                textBox_viewer.AppendText("English Name:\t" + movieName.EnglishName + "\r\n");
                bdindex.Clear();
                bdindex = GetBaiduIndex(movieName.EnglishName);
                textBox_viewer.AppendText("Baidu Index:\r\n");
                if (bdindex.Count == 4)
                {
                    textBox_viewer.AppendText("最近7天整体搜索指数:\t" + bdindex[0] + "\r\n");
                    textBox_viewer.AppendText("最近7天移动搜索指数:\t" + bdindex[1] + "\r\n");
                    textBox_viewer.AppendText("最近30天整体搜索指数:\t" + bdindex[2] + "\r\n");
                    textBox_viewer.AppendText("最近30天移动搜索指数:\t" + bdindex[3] + "\r\n");
                }
                textBox_viewer.AppendText("-------------------------------------------------------\r\n");
                bdindex.Clear();
            }
            textBox_viewer.AppendText("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||\r\n");
            foreach (string movieID in upcomingMovies)
            {
                textBox_viewer.AppendText("ID:\t" + movieID + "\r\n");
                BothName movieName = GetMovieName(GetMovieHome(movieID));
                textBox_viewer.AppendText("Chinese Name:\t" + movieName.ChineseName + "\r\n");
                List<string> bdindex = GetBaiduIndex(movieName.ChineseName);
                textBox_viewer.AppendText("Baidu Index:\r\n");
                if (bdindex.Count == 4)
                {
                    textBox_viewer.AppendText("最近7天整体搜索指数:\t" + bdindex[0] + "\r\n");
                    textBox_viewer.AppendText("最近7天移动搜索指数:\t" + bdindex[1] + "\r\n");
                    textBox_viewer.AppendText("最近30天整体搜索指数:\t" + bdindex[2] + "\r\n");
                    textBox_viewer.AppendText("最近30天移动搜索指数:\t" + bdindex[3] + "\r\n");
                }
                textBox_viewer.AppendText("English Name:\t" + movieName.EnglishName + "\r\n");
                bdindex.Clear();
                bdindex = GetBaiduIndex(movieName.EnglishName);
                textBox_viewer.AppendText("Baidu Index:\r\n");
                if (bdindex.Count == 4)
                {
                    textBox_viewer.AppendText("最近7天整体搜索指数:\t" + bdindex[0] + "\r\n");
                    textBox_viewer.AppendText("最近7天移动搜索指数:\t" + bdindex[1] + "\r\n");
                    textBox_viewer.AppendText("最近30天整体搜索指数:\t" + bdindex[2] + "\r\n");
                    textBox_viewer.AppendText("最近30天移动搜索指数:\t" + bdindex[3] + "\r\n");
                }
                bdindex.Clear();
            }
        }
        private void loginBaiduToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://index.baidu.com/?tpl=trend&word=%D6%BD%C5%C6%CE%DD");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (docCompleted.Keys.Contains(e.Url.AbsoluteUri))
            {
                docCompleted[e.Url.AbsoluteUri] = true;
                
                //docs[e.Url.AbsoluteUri] = GetBaiduIndexByContent(webBrowser1.Document.All["mainWrap"].InnerHtml);

            }

        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }

        private delegate void CrossThreadGetHTML();

        #region wangpiao
        private string getWangpiaoData(DateTime d)
        {
            WebClient client = new WebClient();
            byte[] wdata = client.DownloadData("http://wdbox.sinaapp.com/ajaxcd/wpbox3.php?selectDay=" + d.ToString("yyyy-MM-dd"));
            return System.Text.Encoding.UTF8.GetString(wdata);
        }
        private List<wangpiao> getWangpiaoInfo(string strHtml)
        {
            List<wangpiao> wInfo = new List<wangpiao>();
            string zongchangci = "";
            string tongjichangci = "";
            string changjun = "";
            string shangzuolv = "";
            string renci = "";
            string piaojia = "";
            string piaofang = "";
            string moviename = "";
            string movieid = "";
            int start = strHtml.IndexOf("</tr>");
            int end = 0;
            while (strHtml.IndexOf("<tr", start) > 0)
            {
                start = strHtml.IndexOf("film_id=", start)+8;
                end = strHtml.IndexOf("&", start);
                movieid = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("film_name=", end)+10;
                end = strHtml.IndexOf("'>", start);
                moviename = System.Web.HttpUtility.UrlDecode(strHtml.Substring(start, end - start), System.Text.Encoding.UTF8);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end)+1;
                zongchangci = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                tongjichangci = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                changjun = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                shangzuolv = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                renci = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                piaojia = strHtml.Substring(start, end - start);
                start = strHtml.IndexOf("<td", end);
                end = strHtml.IndexOf("</td>", start);
                start = strHtml.LastIndexOf(">", end) + 1;
                piaofang = strHtml.Substring(start, end - start);
                wangpiao wangpiaoInfo = new wangpiao(zongchangci,tongjichangci,changjun,shangzuolv,renci,piaojia,piaofang,moviename,movieid);
                wInfo.Add(wangpiaoInfo);
                start = strHtml.IndexOf("</tr>", start);
            }


            return wInfo;
        }

        private void recordWangpiao(string strHtml)
        {
            List<wangpiao> wInfo = getWangpiaoInfo(strHtml);
            string insertSql = "INSERT INTO [WANGPIAO_INFO]([MOVIE_ID],[MOVIE_NAME],[ZONGCHANGCI],[TONGJICHANGCI] "+
",[CHANGJUN],[SHANGZUOLV],[RENCI],[PIAOJIA],[PIAOFANG],[RECORD_TIME]) VALUES (@MOVIE_ID "+
",@MOVIE_NAME,@ZONGCHANGCI,@TONGJICHANGCI,@CHANGJUN,@SHANGZUOLV,@RENCI,@PIAOJIA,@PIAOFANG,@RECORD_TIME)";
            using (DataBase db = new DataBase())
            {
                foreach (wangpiao w in wInfo)
                {
                    
                    db.AddParameter("MOVIE_ID", w.movieid);
                    db.AddParameter("MOVIE_NAME", w.moviename);
                    db.AddParameter("ZONGCHANGCI", w.zongchangci);
                    db.AddParameter("TONGJICHANGCI", w.tongjichangci);
                    db.AddParameter("CHANGJUN", w.changjun);
                    db.AddParameter("SHANGZUOLV", w.shangzuolv);
                    db.AddParameter("RENCI", w.renci);
                    db.AddParameter("PIAOJIA", w.piaojia);
                    db.AddParameter("PIAOFANG", w.piaofang);
                    db.AddParameter("RECORD_TIME", DateTime.Now);
                    db.ExecuteNonQuery(insertSql);
                }
            }

        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Hour == 22)
            {
                recordMtimeInfo();
                recordEntgroupMovieInfo();
                recordWangpiao(getWangpiaoData(DateTime.Now));
            }
        }
    }
}
