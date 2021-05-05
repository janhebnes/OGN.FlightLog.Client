namespace OGN.FlightLog.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using System.Net;

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
            /// Returns the fully formatted request url to the flightlog page of live.glidernet.org located at ktrax.kisstech.ch
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"https://ktrax.kisstech.ch/logbook/?id={AirfieldParameter}&tz={TimeZoneParameter}&day={DateParameter}&units={UnitParameter}&shorthand=true&showtype=true&fstatus=all&ftype=all&disp=cs&showcrew=false";
            }

            /// <summary>
            /// Returns the CSV download Address
            /// </summary>
            /// <returns></returns>
            public string ToCsvDownloadAddress()
            {
                return   $"https://ktrax.kisstech.ch/backend/logbook/?csv=1&id={AirfieldParameter}&dbeg={DateParameter}&dend={DateParameter}&tz=2";
            }

            /// <summary>
            /// Returns the JSON Address
            /// </summary>
            /// <returns></returns>
            public string ToJsonAddress()
            {
                return $"https://ktrax.kisstech.ch/backend/logbook/?id={AirfieldParameter}&tz={TimeZoneParameter}&dbeg={DateParameter}&dend={DateParameter}&db=sortie&query_type=ap";
            }

            /// <summary>
            /// Generates a unique identifier for the current options
            /// </summary>
            /// <returns></returns>
            public string GetDatasetIdentifier()
            {
                return this.Airfield + this.DateParameter + "z" + this.TimeZoneParameter + this.UnitParameter;
            }

            public DateTime Date = DateTime.Today;
            internal string DateParameter { get { return Date.ToString("yyyy-MM-dd"); } }

            public string Airfield;
            internal string AirfieldParameter { get { return Airfield; } }

            public unit Unit = unit.metric;
            internal string UnitParameter
            {
                get
                {
                    switch (Unit)
                    {
                        case unit.metric:
                            return "metric";
                        case unit.imperial:
                            return "imperial";
                    }
                    return "metric";
                }
            }

            /// <summary>
            /// TimeZone Offset from UTC
            /// </summary>
            /// <remarks>Remember to handle summertime offset</remarks>
            public int TimeZone = 2;
            internal string TimeZoneParameter { get { return TimeZone.ToString(); } }

            /// <summary>
            /// WebClient timeout 
            /// </summary>
            public int Timeout = 1500;
        }

        public enum unit { metric, imperial };

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
        /// <param name="useLocalDatabaseCache"></param>
        /// <returns></returns>
        public static List<Flight> GetFlights(Options options, bool useLocalDatabaseCache = true)
        {
            if (!useLocalDatabaseCache)
                return GetLiveFlights(options);

            if (options.Date == DateTime.Now.Date)
                return GetLiveFlights(options); //GetChangeTrackedFlights(options, GetLiveFlights(options));

            using (var db = new FlightLogContext())
            {
                var datasetIdentifier = options.GetDatasetIdentifier();
                var flights = db.Logbook.Where(f => f.dataset == datasetIdentifier);
                if (flights.Any())
                {
                    if (flights.Any(ZeroFlightDayMarker)) 
                        return new List<Flight>();

                    return flights.ToList();
                }

                var result = GetLiveFlights(options);
                if (!result.Any())
                {
                    result.Add(ZeroFlightDayMarker(options));
                }

                db.Logbook.AddRange(result);
                db.SaveChanges();
                return result;
            }
        }

        /// <summary>
        /// Days with zero flights are stored with one flight with row -1;
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        internal static bool ZeroFlightDayMarker(Flight f) => f.row == -1;

        /// <summary>
        /// A zero flight marker with row -1 (for avoiding calling the live webservice on zero flight days)
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static Flight ZeroFlightDayMarker(Options options) => new Flight(options, -1);

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
            WebClient client = new WebClientWithTimeout(options.Timeout);
            string csv = client.DownloadString(options.ToCsvDownloadAddress());
            if (!csv.StartsWith(Flight.Header))
            {
                throw new System.IO.FileNotFoundException("Invalid file format returned when retrieving csv information for airport " + options.Airfield + " at " + options.ToString(), "Airfield");
            }

            var result = new List<Flight>();
            int row = 0;
            foreach (string line in csv.Split('\n')) // The CSV returns line feed char 10 aka \n
            {
                bool IsMetaDataFooterSection = line.StartsWith(string.Intern("BEGIN_DATE,") + options.DateParameter);
                if (IsMetaDataFooterSection)
                    break;

                if (row++ == 0) continue;

                result.Add(new Flight(options, row++, line));
            }

            return result;
        }

        public class WebClientWithTimeout : WebClient
        {
            private int _timeout = 5000;

            public WebClientWithTimeout(int timeout) : base()
            {
                _timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = _timeout; // timeout in milliseconds (ms)
                return wr;
            }
        }

        #region sync feature is put on ice
        /////// <summary>
        /////// update database store and add ChangeTracked state information to the returned flight information
        /////// </summary>
        /////// <param name="options"></param>
        /////// <returns></returns>
        /////// <remarks>Change tracking is implemented by monitoring .ChangeTracker on the db context</remarks>
        ////public static List<Flight> GetChangeTrackedFlights(Options options, List<Flight> flights)
        ////{
        ////    var liveFlights = GetLiveFlights(options);
        ////    return liveFlights;
        ////    //var savedFlights = db.Flights.Where(f => f.dataset == datasetIdentifier);
        ////    //db.Flights.AddRange(liveFlights);
        ////    //db.SaveChanges();



        ////    //////this.ChangeTracker.Entries<Flight>().Where(f => (f.State == EntityState.Added) || (f.State == EntityState.Deleted) || (f.State == EntityState.Modified))
        ////    //////.ToList<DbEntityEntry<Flight>>()
        ////    //////.ForEach((c => this.FlightVersions.Add(new FlightVersionHistory((Flight)c.Entity, c.State))));

        ////    //return liveFlights; // and change trackings?
        ////}
        #endregion
    }
}
