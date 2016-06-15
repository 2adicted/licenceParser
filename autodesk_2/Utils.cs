using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenceParser
{
    class Utils
    {
        #region utils
        internal static Dictionary<string, int> timeParse = new Dictionary<string, int>()
        {
            {"Jan", 1},
            {"Feb", 2},
            {"Mar", 3},
            {"Apr", 4},
            {"May", 5},
            {"Jun", 6},
            {"Jul", 7},
            {"Aug", 8},
            {"Sep", 9},
            {"Oct", 10},
            {"Nov", 11},
            {"Dec", 12}
        };
        internal static Dictionary<int, string> weekParse = new Dictionary<int, string>()
        {
            {0, "Mon"},
            {1, "Tue"},
            {2, "Wed"},
            {3, "Thu"},
            {4, "Fri"},
            {5, "Sat"},
            {6, "Sun"}
        };
        /// <summary>
        /// retrieves a list of strings if they match ceratin criteria
        /// </summary>
        /// <param name="p"></param>
        /// <param name="licenceLines"></param>
        /// <returns></returns>
        internal static List<string> get_string(string p, List<string> licenceLines)
        {
            return licenceLines.Where(s => s.Contains(p)).ToList();
        }
        /// <summary>
        /// color for each day of the week
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        internal static Color select_color(int i)
        {
            Color[] colors = new Color[7];
            colors[0] = Color.FromArgb(90, 200, 200, 30);
            colors[1] = Color.FromArgb(60, 100, 200, 130);
            colors[2] = Color.FromArgb(30, 200, 200, 30);
            colors[3] = Color.FromArgb(90, 200, 200, 30);
            colors[4] = Color.FromArgb(60, 100, 200, 130);
            colors[5] = Color.FromArgb(30, 200, 200, 30);
            colors[6] = Color.FromArgb(90, 200, 200, 30);

            return colors[i % 7];
        }
        /// <summary>
        /// concatenate hour and minute input into a string
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        internal static string makeTime(int i, int j)
        {
            string hours = i.ToString();
            if (hours.Length < 2) hours = "0" + hours;
            string minutes = j.ToString();
            if (minutes.Length < 2) minutes = "0" + minutes;

            return (hours + ":" + minutes + ":" + "00");
        }
        /// <summary>
        /// the oposite of makeTime above
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static TimeSpan breakTime(string t)
        {
            TimeSpan now = new TimeSpan(0, 0, 0);
            string[] t_string = t.Split(':');
            if (t_string[0].Length < 2) t_string[0] = "0" + t_string[0];
            now = new TimeSpan(Convert.ToInt32(t_string[0]), Convert.ToInt32(t_string[1]), Convert.ToInt32(t_string[2]));

            return now;
        }
        /// <summary>
        /// returns the timestamp
        /// format is Month/Date/Year
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal static DateTime stampParse(string l)
        {
            string[] parse = l.Split(' ').Last().Split('/');
            int year = Convert.ToInt32(parse[2]);
            int month = Convert.ToInt32(parse[0]);
            int day = Convert.ToInt32(parse[1]);
            DateTime stamp = new DateTime(year, month, day);
            return stamp;
        }
        /// <summary>
        /// random color, not used
        /// </summary>
        /// <returns></returns>
        private Color random_color()
        {
            Random randomGen = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[randomGen.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);
            if (randomColor.ToString().Contains("light")) randomColor = random_color();

            return randomColor;
        }
        /// <summary>
        /// num of users, not used
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        private void num_users(string p, int d, List<Licence> studio_licences, System.Windows.Forms.Label label16)
        {
            IEnumerator licences = studio_licences.GetEnumerator();
            int active_licences = 0;
            while (licences.MoveNext())
            {
                Licence l = (Licence)licences.Current;
                if (l.active(p, d)) active_licences++;
            }
            label16.Text = active_licences.ToString();
        }
        #endregion
        internal static string look_up_month(string report_month)
        {

            if (report_month.Contains("mar") || report_month.Contains("Mar")) return "3";
            else return "0";
        }
    }
}
