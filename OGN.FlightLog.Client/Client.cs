namespace OGN.FlightLog.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using System.Net;
    using Newtonsoft.Json.Linq;
    
    public class Client
    {
        public class Options
        {
            public Options(string airfield) 
            {
                this.Airfield = airfield;
                this.TimeZone = Convert.ToInt32(TimeZoneInfo.Local.BaseUtcOffset.TotalHours);
            }
            
            public Options(string airfield, int timeZone)
            {
                this.Airfield = airfield;
                this.TimeZone = timeZone;
            }

            public Options(string airfield, DateTime date) : this(airfield)
            {
                this.Date = date;
            }
            public Options(string airfield, int timeZone, DateTime date) : this(airfield, timeZone)
            {
                this.Date = date;
            }

            /// <summary>
            /// Returns the fully formatted request url to the flightlog page of live.glidernet.org
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"http://live.glidernet.org/flightlog/index.php?a={AirfieldParameter}&s={Alt_settingParameter}&u={UnitParameter}&z={TimeZoneParameter}&p=&d={DateParameter}&j";
            }

            public DateTime Date = DateTime.Today;
            internal string DateParameter { get { return Date.ToString("ddMMyyyy"); } }

            public string Airfield;
            internal string AirfieldParameter { get { return Airfield; } }

            public alt_settings Alt_setting = alt_settings.QFE;
            internal string Alt_settingParameter
            {
                get
                {
                    switch (Alt_setting)
                    {
                        case alt_settings.QFE:
                            return "QFE";
                        case alt_settings.QNE:
                            return "QNE";
                        case alt_settings.QNH:
                            return "QNH";
                    }
                    return "QFE";
                }
            }
            public unit Unit = unit.meter;
            internal string UnitParameter
            {
                get
                {
                    switch (Unit)
                    {
                        case unit.meter:
                            return "M";
                        case unit.feet:
                            return "F";
                    }
                    return "M";
                }
            }

            public int TimeZone = 2;
            internal string TimeZoneParameter { get { return TimeZone.ToString(); } }

        }

        public enum unit { meter, feet };

        /// <summary>
        /// QFE Height above airfield
        /// QNH Height above sea level.
        /// QNE Height above sea level at standard setting 1013
        /// </summary>
        public enum alt_settings { QFE, QNH, QNE };

        public Options options;

        public Client(string airfield)
        {
            options = new Options(airfield);
        }

        public Client(string airfield, int timeZone)
        {
            options = new Options(airfield, timeZone);
        }

        /// <summary>
        /// Fetch the flight information based
        /// </summary>
        /// <param name="date"></param>
        /// <param name="enableLocalDbCache">if local db cache is enabled flights that have been cached and that are not from today are fetch from the local database</param>
        /// <returns></returns>
        public List<Flight> GetFlights(DateTime date, bool enableLocalDbCache = true)
        {
            options.Date = date;

            // Passes the request along to the static counterpart
            return GetFlights(options, enableLocalDbCache);
        }

        /// <summary>
        /// Optionaly you can use the static directly 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="enableLocalDbCache"></param>
        /// <returns></returns>
        public static List<Flight> GetFlights(Options options, bool enableLocalDbCache = true)
        {
            if (!enableLocalDbCache)
                return GetLiveFlights(options);

            return null;

            // TODO: Do we create a new Db Context for Ogn log data .. ?<s
            //  app.CreatePerOwinContext(ApplicationDbContext.Create);
            //  context.Get<ApplicationDbContext>() that allows for a single instance to be used all over... 
        }

        /// <summary>
        /// We are getting the results live from from http://live.glidernet.org/flightlog/index.php?a=EHDL&s=QFE&u=M&z=2&p=&d=30052015&j 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <remarks>
        /// JSON Parsing is done with Newtonsoft.Json with the same approach as described on this post
        /// http://stackoverflow.com/questions/32401704/appending-json-data-to-listview-c-sharp
        /// </remarks>
        internal static List<Flight> GetLiveFlights(Options options)
        {
            var result = new List<Flight>();
            
            WebClient client = new WebClient();
            string json = client.DownloadString(options.ToString());
            JObject data = JObject.Parse(json);

            var items = data["flights"]
                .Children<JObject>()
                .Select(jo => new Flight(options, jo));

            result.AddRange(items);
            
            return result;
        }

    }
}
