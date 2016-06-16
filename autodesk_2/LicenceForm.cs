using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using LicenceParser;

namespace LicenceParser
{
    public partial class LicenceForm : Form
    {
        public List<string> licenceLines = new List<string>();
        public List<string> licenceOut = new List<string>();
        public List<string> licenceIn = new List<string>();
        
        Dictionary<string, string> flexNames = new Dictionary<string, string>()
        {
            {"86071BDSPRM_2014_0F", "Autodesk Building Design Suite Premium 2014"},
            {"86274REVIT_2015_0F", "Autodesk Revit Architecture 2015"},
            {"86273RVT_2015_0F", "Autodesk Revit 2015"},
            {"86275RVTLT_2015_0F", "Autodesk Revit LT 2015"},
            {"86238BDSPRM_2015_0F", "Autodesk Building Design Suite Premium 2015"},
            {"86454REVIT_2016_0F", "Autodesk Revit Architecture 2016"},
            {"86453RVT_2016_0F", "Autodesk Revit 2016"},
            {"86455RVTLT_2016_0F", "Autodesk Revit LT 2016"},
            {"86451BDSPRM_2016_0F", "Autodesk Building Design Suite Premium 2016"},
            {"86706RVT_2017_0F", "Autodesk Revit 2017"},
            {"86707RVTLT_2017_0F", "Autodesk Revit LT 2017"},
            {"86696BDSPRM_2017_0F", "Autodesk Building Design Suite Premium 2017"},
        };

        List<Licence> studio_licences = new List<Licence>();        

        private int days;
        private DateTime day;
        private DateTime timeStart;
        private DateTime timeEnd;
        private bool opened = false;
        private List<int> globalUsage = new List<int>();
        private Color mainColor = Color.FromArgb(5, 0, 0, 0);
        private Color secondaryColor = Color.FromArgb(1, 0, 0, 0);
        private int scale = 1;


        public LicenceForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// load the debug file (in txt format)
        /// and do all the work in this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Start:
                if (!opened)
                {
                    opened = true;
                    OpenFileDialog theDialog = new OpenFileDialog();
                    theDialog.Title = "Open Text File";
                    theDialog.Filter = "TXT files|*.txt";
                    theDialog.InitialDirectory = @"C:\Users\adicted\Documents\Visual Studio 2012\Projects\autodesk\autodesk";

                    if (theDialog.ShowDialog() == DialogResult.OK)
                    {
                        // make calculation button available now
                        maxUsageBtn.Visible = true;

                        string filename = theDialog.FileName;
                        string title = theDialog.SafeFileName;

                        string[] filelines = File.ReadAllLines(filename);

                        int num = 0;

                        List<Label> licenceLabels = new List<Label>();

                        licenceLabels.Add(label25);
                        licenceLabels.Add(label26);
                        licenceLabels.Add(label27);
                        licenceLabels.Add(label28);
                        licenceLabels.Add(label29);
                        licenceLabels.Add(label30);
                        licenceLabels.Add(label31);

                        Boolean time_set = false;
                        for (int i = 0; i < filelines.Length; i++)
                        {
                            foreach (KeyValuePair<string, string> entry in flexNames)
                            {
                                if (filelines[i].Contains(entry.Key) || filelines[i].Contains("TIMESTAMP"))
                                {
                                    if(!licenceLabels.Any(l => l.Text.Contains(entry.Value)))
                                    {
                                        if (filelines[i].Contains("TIMESTAMP")) continue;
                                        var label = licenceLabels.First(l => l.Text.Equals("0"));
                                        label.Text = entry.Value;
                                        label.Visible = true;
                                    }
                                    licenceLines.Add(filelines[i]);
                                    num++;
                                }
                            }
                            // The starting date of the log
                            if (!time_set && filelines[i].Contains("Start-Date:"))
                            {
                                time_set = true;
                                string[] t = filelines[i].Split(' ');
                                timeStart = new DateTime(Convert.ToInt32(t[7]), Convert.ToInt32(Utils.timeParse[t[5]]), Convert.ToInt32(t[6]));
                                label14.Text = "File Name: " + title + System.Environment.NewLine + "Report log date: " + timeStart.ToString("dddd") + ", " + timeStart.ToString("dd/MMM/yyyy");
                            }
                        }
                        label6.Text = num.ToString();
                    }
                    else
                    {
                        opened = false;
                        MessageBox.Show("No file loaded. Please load a valid debug log file.");
                    }
                    //the main method that extracts the data
                    dataCrunch(licenceLines);

                    licenceOut = Utils.get_string("OUT:", licenceLines);
                    licenceIn = Utils.get_string("IN:", licenceLines);
                    List<string> licenceBounces = Utils.get_string("UNSUPPORTED:", licenceLines);
                    List<string> licenceRegections = Utils.get_string("DENIED:", licenceLines);
                    List<string> licenceRandom = Utils.get_string("Checkin", licenceLines);
                    licenceRandom.AddRange(Utils.get_string("consisting of:", licenceLines));

                    label2.Text = licenceOut.Count().ToString();
                    label3.Text = licenceIn.Count().ToString();
                    label8.Text = licenceBounces.Count().ToString();
                    label11.Text = licenceRegections.Count().ToString();
                    label20.Text = "Current Date: " + timeStart.ToString("dddd") + ", " + timeStart.ToString("dd/MMM/yyyy");
                }
                else
                {
                    this.Controls.Clear();
                    WipeClean();
                    InitializeComponent();
                    opened = false;
                    goto Start;
                }
        }
        /// <summary>
        /// parse the data from the log file
        /// </summary>
        /// <param name="licenceLines"></param>
        private void dataCrunch(List<string> licenceLines)
        {
            days = 0;
            //if datytime is true, then it's between 0-9. if its false its between 10-23)
            List<string> s = new List<string>();

            IEnumerator lines = licenceLines.GetEnumerator();

            TimeSpan current = new TimeSpan();
            TimeSpan previous = new TimeSpan();
            DateTime timeStamp = new DateTime();
            DateTime now = timeStart;
            bool change = false;

            while (lines.MoveNext())
            {
                string l = (string)lines.Current;
                if (l.Contains("TIMESTAMP"))
                {
                    timeStamp = Utils.stampParse(l);
                    if (!timeStamp.Equals(now))
                    {
                        days++;
                        change = false;
                    }
                    if (!change)
                    {
                        now = timeStamp;
                        timeEnd = timeStamp;
                        change = true;
                    }
                }
                if (l.Contains("OUT:"))
                {
                    string[] parse_it = l.Split(' ');

                    if (parse_it.Length == 7)
                    {
                        studio_licences.Add(new Licence(parse_it[0], parse_it[3], parse_it[4], this.days, now + new TimeSpan(Utils.hours(parse_it[0]), Utils.minutes(parse_it[0]), Utils.seconds(parse_it[0]))));
                        current = Utils.breakTime(parse_it[0]);
                    }
                    else if (parse_it.Length == 8)
                    {
                        studio_licences.Add(new Licence(parse_it[1], parse_it[4], parse_it[5], this.days, now + new TimeSpan(Utils.hours(parse_it[1]), Utils.minutes(parse_it[1]), Utils.seconds(parse_it[1]))));
                        current = Utils.breakTime(parse_it[1]);
                    }
                    //if (current < previous) days++;
                    previous = current;
                }
                else if (l.Contains("IN:"))
                {
                    string[] parse_it = l.Split(' ');
                    if (parse_it.Length == 7)
                    {
                        kill_licences(parse_it[0], parse_it[3], parse_it[4], now + new TimeSpan(Utils.hours(parse_it[0]), Utils.minutes(parse_it[0]), Utils.seconds(parse_it[0])));
                    }
                    else if (parse_it.Length == 8)
                    {
                        kill_licences(parse_it[1], parse_it[4], parse_it[5], now + new TimeSpan(Utils.hours(parse_it[1]), Utils.minutes(parse_it[1]), Utils.seconds(parse_it[1])));
                    }
                }
            }

            label15.Text = days.ToString();
        }
        /// <summary>
        /// max usage - start calculating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click_1(object sender, EventArgs e)
        {
            this.progressBar1.Visible = true;
            SplineChart();
            PieChart();
            this.progressBar1.Visible = false;
            this.maxUsageBtn.Visible = false;
        }        
        /// <summary>
        /// retrieve concurent number of users for given date
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private int num_users_int(string p, int d, DateTime c)
        {
            int licences = (studio_licences.Where(l => l.active(p, d, c)).Count());
            globalUsage.Add(licences);
            return licences;
        } 
        /// <summary>
        /// kill the licence instance
        /// </summary>
        /// <param name="time_death"></param>
        /// <param name="type"></param>
        /// <param name="user"></param>
        private void kill_licences(string time_death, string type, string user, DateTime end)
        {
            IEnumerator licences = studio_licences.GetEnumerator();
            while (licences.MoveNext())
            {
                Licence l = (Licence)licences.Current;

                if (l.get_user().Equals(user) && !l.closed)
                {
                    l.set_time_death(time_death, end);
                    l.closed = true;
                }
            }
        }
        /// <summary>
        /// start clean funciton
        /// </summary>
        private void WipeClean()
        {
            licenceLines = new List<string>();
            licenceOut = new List<string>();
            licenceIn = new List<string>();
            studio_licences = new List<Licence>();
            days = 0;
            day = new DateTime();
            timeStart = new DateTime();
            opened = false;
            globalUsage = new List<int>();
        }
        private void label20_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if(me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IncrementUp();
            } 
            else if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                IncrementDown();
            }
        }
        private void IncrementUp()
        {
            day = day.AddDays(1);
            if (DateTime.Compare(day, timeEnd) == 0) day = timeStart;
            label20.Text = "Current Date: " + day.ToString("dddd") + ", " + day.ToString("dd/MMM/yyyy");
            if(licence_usage_chart.Series.Count > 0) overrideColor(day);
        }
        private void IncrementDown()
        {
            day = day.AddDays(-1);
            if (DateTime.Compare(day, timeStart) < 0) day = timeEnd;
            label20.Text = "Current Date: " + day.ToString("dddd") + ", " + day.ToString("dd/MMM/yyyy");
            if (licence_usage_chart.Series.Count > 0) overrideColor(day);
        }
        private void overrideColor(DateTime day)
        {
            foreach (Series s in licence_usage_chart.Series)
            {
                s.Color = secondaryColor;
            }
            licence_usage_chart.Series.Where(s => s.Name.Contains(day.Day.ToString() + " " + day.ToString("MMM"))).First().Color = Color.FromArgb(60, 138, 205, 230);
        }

        #region Buttons
        /// <summary>
        /// Export chart as an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String saved_file = "";
            Bitmap objDrawingSurface;        
            Rectangle rectBounds1;

            saveLocation.Title = "Choose save location";
            saveLocation.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveLocation.FileName = "";
            saveLocation.Filter = "JPEG IMAGES|*.jpg|PNG Images|*.png";

            if (saveLocation.ShowDialog() == DialogResult.Cancel) MessageBox.Show("Operation Cancelled");
            else
            {
                saved_file = saveLocation.FileName;

                objDrawingSurface = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
                rectBounds1 = new Rectangle(0, 0, this.Width, this.Height);
                this.DrawToBitmap(objDrawingSurface, rectBounds1);
                objDrawingSurface.Save(saved_file, System.Drawing.Imaging.ImageFormat.Png);

                //licence_usage_chart.SaveImage(saved_file, ChartImageFormat.Png);
            }
        }     
        /// <summary>
        /// not used currently
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.Increment(1);
        }
        /// <summary>
        /// Quit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Charts
        /// <summary>
        /// Create spline chart
        /// </summary>
        private void SplineChart()
        {
            this.licence_usage_chart.Series.Clear();

            this.licence_usage_chart.Titles.Add("Licence Usage over Time");

            List<Series> series = new List<Series>();

            DateTime current_time = timeStart;

            for (int i = 0; i < days; i++)
            {
                series.Add(new Series(current_time.DayOfWeek.ToString() + ", " + current_time.Day.ToString() + " " + current_time.ToString("MMM")));
                current_time = current_time.AddDays(1);
            }
            current_time = timeStart;
            int counter = 0;

            IEnumerator s_series = series.GetEnumerator();

            InitializeChart();

            progressBar1.Minimum = 1;
            progressBar1.Maximum = series.Count;
            progressBar1.Value = 1;
            progressBar1.Step = 1;

            Series s = new Series();
            s.ChartType = SeriesChartType.SplineArea;
            s.BorderWidth = 1;
            //s.ShadowOffset = 1;
            s.ShadowColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);

            while (s_series.MoveNext())
            {
                if (counter < days)
                {
                    s = (Series)s_series.Current;

                    s.IsXValueIndexed = true;
                    s.XValueType = ChartValueType.Time;
                    //s.Color = Utils.select_color(counter);
                    day = timeStart;
                    if (s.Name.Contains("Sun") || s.Name.Contains("Sat")) s.Color = Color.FromArgb(60, 138, 205, 230);
                    else s.Color = s.Color = mainColor;

                    add_point_to_chart(s, current_time, counter);
                    licence_usage_chart.Series.Add(s);
                    current_time = current_time.AddDays(1);
                }
                counter++;
                progressBar1.PerformStep();
            }
        }

        /// <summary>
        /// calculate the overall distribution of licences
        /// </summary>
        private void PieChart()
        {
            globalUsage.RemoveAll(i => i == 0);
            int max = globalUsage.Max();
            int newMax = max;
            Dictionary<int, int> percentageUsage = new Dictionary<int, int>();
            for (int i = 1; i < max; i++)
            {
                int percentage = (int)((double)globalUsage.Where(j => j == i).ToList().Count / (double)(globalUsage.Count) * 100);
                if (percentage >= 1) percentageUsage.Add(i, percentage);
                else newMax--;
            }

            this.licence_usage_piechart.Series.Clear();

            this.licence_usage_piechart.Titles.Add("Licence Distribution");
            this.licence_usage_piechart.Titles.First().Alignment = ContentAlignment.MiddleLeft;

            progressBar1.Minimum = 1;
            progressBar1.Maximum = newMax;
            progressBar1.Value = 1;
            progressBar1.Step = 1;

            InitializePieChart();

            Series series = new Series();

            series.Points.Clear();
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series.CustomProperties = "PieLabelStyle=Outside";

            foreach (KeyValuePair<int, int> entry in percentageUsage)
            {
                series.Points.AddXY(entry.Key.ToString() + ": " + entry.Value.ToString() + "%", entry.Value);
                progressBar1.PerformStep();
            }
            //for (int i = 1; i < newMax; i++)
            //{
            //    series.Points.AddXY(i.ToString() + ": " + percentageUsage[i].ToString() + "%", percentageUsage[i]);
            //    progressBar1.PerformStep();
            //}

            licence_usage_piechart.Series.Add(series);
        }
        /// <summary>
        /// Chart settings
        /// </summary>
        private void InitializeChart()
        {
            this.licence_usage_chart.ResetAutoValues();
            this.licence_usage_chart.Visible = true;
            this.licence_usage_chart.ChartAreas[0].AxisX.LabelStyle.Interval = 60;
            this.licence_usage_chart.ChartAreas[0].AxisX.Maximum = 1440;
            this.licence_usage_chart.ChartAreas[0].AxisX.MajorGrid.Interval = 60;
            this.licence_usage_chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 60;
            this.licence_usage_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightBlue;
            this.licence_usage_chart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            this.licence_usage_chart.ChartAreas[0].AxisX.IsMarginVisible = false;

            this.licence_usage_chart.ChartAreas[0].AxisY.Minimum = 0;
            this.licence_usage_chart.ChartAreas[0].AxisY.Maximum = 16*scale;
            this.licence_usage_chart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
            this.licence_usage_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightSalmon;
            this.licence_usage_chart.ChartAreas[0].AxisY.MinorGrid.Interval = 4*scale;
            this.licence_usage_chart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            this.licence_usage_chart.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            this.licence_usage_chart.ChartAreas[0].AxisY.MajorGrid.Interval = 4*scale;
            this.licence_usage_chart.ChartAreas[0].AxisY.MajorTickMark.Interval = 4*scale;
            this.licence_usage_chart.ChartAreas[0].AxisY.LabelStyle.Interval = 4*scale;

            //this.licence_usage_chart.ChartAreas[0].BackColor = System.Drawing.Color.Transparent;

            this.licence_usage_chart.AntiAliasing = AntiAliasingStyles.All;
            this.licence_usage_chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
            this.licence_usage_chart.ChartAreas[0].ShadowOffset = 2;

            this.licence_usage_chart.Legends[0].BackColor = System.Drawing.Color.Transparent;
        }

        /// <summary>
        /// set up pie chart
        /// </summary>
        private void InitializePieChart()
        {
            this.licence_usage_piechart.ResetAutoValues();
            this.licence_usage_piechart.Visible = true;
            this.licence_usage_piechart.ChartAreas[0].AxisX.LabelStyle.Interval = 60;
            this.licence_usage_piechart.ChartAreas[0].AxisX.Maximum = 100;
            //this.licence_usage_chart.ChartAreas[0].AxisX.MajorGrid.Interval = 60;
            this.licence_usage_piechart.ChartAreas[0].AxisX.MajorTickMark.Interval = 60;
            this.licence_usage_piechart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightBlue;
            this.licence_usage_piechart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            this.licence_usage_piechart.ChartAreas[0].AxisX.IsMarginVisible = false;

            this.licence_usage_piechart.ChartAreas[0].AxisY.Minimum = 0;
            this.licence_usage_piechart.ChartAreas[0].AxisY.Maximum = 36;
            this.licence_usage_piechart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
            this.licence_usage_piechart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightSalmon;
            this.licence_usage_piechart.ChartAreas[0].AxisY.MinorGrid.Interval = 4;
            this.licence_usage_piechart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            this.licence_usage_piechart.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

            this.licence_usage_piechart.ChartAreas[0].BackColor = System.Drawing.Color.Transparent;

            this.licence_usage_piechart.AntiAliasing = AntiAliasingStyles.All;
            this.licence_usage_piechart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
            //this.licence_usage_piechart.ChartAreas[0].ShadowOffset = 2;

            this.licence_usage_piechart.Legends[0].BackColor = System.Drawing.Color.Transparent;
        }
        /// <summary>
        /// Populates series for the chart
        /// Each cycle represents one day
        /// </summary>
        /// <param name="series"></param>
        /// <param name="current"></param>
        /// <param name="d"></param>
        private void add_point_to_chart(Series series, DateTime current, int d)
        {
            //per day
            int[] hour_range = new int[24];
            for (int i = 0; i < 23; i++) hour_range[i] = i;
            int[] minute_range = new int[60];
            for (int i = 0; i < 59; i += 10) minute_range[i] = i;
            int counter = 0;

            DateTime _current = current;
            DateTime _end = _current.AddDays(1);
            
            int[] noisy_data = new int[hour_range.Length * minute_range.Length];

            while (_current.Date != _end.Date)
            {
                string s = Utils.makeTime(_current.Hour, _current.Minute);
                noisy_data[counter] = num_users_int(s, d, _current);
                _current = _current.AddMinutes(1);
                counter++;
            }
            Smoother smooth = new Smoother();
            smooth.set_noisy(noisy_data);
            double[] clean_data = smooth.get_clean();
            _current = current;
            for (int i = 0; i < clean_data.Length; i++)
            {
                //if (clean_data[i] > 40.0) clean_data[i] = 40.0; //clamp the date in terms of max licences 
                series.Points.AddXY(_current, clean_data[i]);
                _current = _current.AddMinutes(1);
            }
        }
        #endregion
    }
}
