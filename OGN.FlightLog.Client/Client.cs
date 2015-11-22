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
        public class InvalidAirportException : ArgumentException
        {
            public InvalidAirportException(string message, string paramName) : base(message, paramName) { }
        }

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

            /// <summary>
            /// Generates a unique identifier for the current options
            /// </summary>
            /// <returns></returns>
            public string GetDatasetIdentifier()
            {
                return this.Airfield + this.DateParameter + this.Alt_settingParameter + "z" + this.TimeZoneParameter + this.UnitParameter;
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
        /// <param name="useLocalDatabaseCache"></param>
        /// <returns></returns>
        public static List<Flight> GetFlights(Options options, bool useLocalDatabaseCache = true)
        {
            if (!useLocalDatabaseCache)
                return GetLiveFlights(options);

            if (options.Date == DateTime.Now.Date)
                return GetChangeTrackedFlights(options, GetLiveFlights(options));

            using (var db = new FlightLogContext())
            {
                var datasetIdentifier = options.GetDatasetIdentifier();
                var flights = db.Flights.Where(f => f.dataset == datasetIdentifier);
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

                db.Flights.AddRange(result);
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
            var result = new List<Flight>();

            WebClient client = new WebClient();
            string json = client.DownloadString(options.ToString());
            if (json.StartsWith("<HTML>"))
            {
                throw new InvalidAirportException("Unable to retrieve feed information for airport " + options.Airfield + " at " + options.ToString(), "Airfield");
            }

            JObject data = JObject.Parse(json);
            int row = 0;
            var items = data["flights"]
                .Children<JObject>()
                .Select(jo => new Flight(options, row++, jo));

            result.AddRange(items);

            return result;
        }

        /// <summary>
        /// update database store and add ChangeTracked state information to the returned flight information
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <remarks>Change tracking is implemented by monitoring .ChangeTracker on the db context</remarks>
        public static List<Flight> GetChangeTrackedFlights(Options options, List<Flight> flights)
        {
            var liveFlights = GetLiveFlights(options);
            return liveFlights;
            //var savedFlights = db.Flights.Where(f => f.dataset == datasetIdentifier);
            //db.Flights.AddRange(liveFlights);
            //db.SaveChanges();



            //////this.ChangeTracker.Entries<Flight>().Where(f => (f.State == EntityState.Added) || (f.State == EntityState.Deleted) || (f.State == EntityState.Modified))
            //////.ToList<DbEntityEntry<Flight>>()
            //////.ForEach((c => this.FlightVersions.Add(new FlightVersionHistory((Flight)c.Entity, c.State))));

            //return liveFlights; // and change trackings?
        }

    }
}
