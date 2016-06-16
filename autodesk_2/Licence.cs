using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenceParser
{
    public class Licence
    {
        public string type, time_birth, user, time_death;
        public bool closed = false;
        public int days;
        //TimeSpan start = new TimeSpan(0,0,0,0);
        //TimeSpan end = new TimeSpan(0,0,0,0);
        DateTime startDateTime { get; set; }
        DateTime endDateTime { get; set; }

        public Licence(string t_b, string t, string u, int d, DateTime s)
        {
            this.time_birth = t_b;
            this.type = t;
            this.user = u;
            this.days = d;
            this.startDateTime = s;
        }

        public Licence()
        { 
        }

       
        /// <summary>
        /// not used
        /// </summary>
        /// <param name="t"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private TimeSpan break_time(string t, int d)
        {
            TimeSpan now = new TimeSpan(0,0,0,0);
            if (t != null)
            {
                string[] t_string = t.Split(':');
                if (t_string[0].Length < 2) t_string[0] = "0" + t_string[0];
                now = new TimeSpan(d, Convert.ToInt32(t_string[0]), Convert.ToInt32(t_string[1]), Convert.ToInt32(t_string[2])) ;

                return now;
            }
            else return now;
        }

        internal string get_user()
        {
            return user;
        }

        internal void set_time_death(string time_death, DateTime e)
        {
            this.time_death = time_death;
            this.endDateTime = e;
        }

        internal bool active(string p, int d, DateTime c)
        {
            Boolean is_active = false;
            DateTime current = c;
            //TimeSpan now = break_time(p, d);
            //DateTime target = new DateTime(2016, 02, 02, 09, 30, 00);
            //bool milka = false;
            //if(DateTime.Compare(current, target) == 0) milka = true;
            if ((DateTime.Compare(current, startDateTime) > 0) && (DateTime.Compare(current, endDateTime) < 0))
            {
                is_active = true;
            }
            else is_active = false;
            return is_active;   
        }

        public void form_timecode()
        {
            //start = break_time(time_birth, days);
            //end = break_time(time_death, days);          
        }
    }
}
