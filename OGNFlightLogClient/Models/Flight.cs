using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OGN.FlightLog.Client.Models
{
    public class Flight
    {
        public Flight(Client.Options options, JObject jo)
        {
            // FROM options
            //   "date": "30052015",
            //   "airfield": "EHDL",
            //   "alt_setting": "QFE",
            //   "unit": "m",
            this.Date = options.Date.Date;
            this.airfield = options.AirfieldParameter;
            this.alt_setting = options.Alt_settingParameter;
            this.unit = options.UnitParameter;
            this.timezone = options.TimeZoneParameter;

            // FROM JObject
            //   "plane": "",
            //   "glider": "6fb4f001",
            //   "takeoff": "11:26:04",
            //   "plane_landing": "",
            //   "glider_landing": "11:31:56",
            //   "plane_time": "-----",
            //   "glider_time": "00h05m52s",
            //   "towplane_max_alt": ""
            this.plane = (string)jo["plane"];
            this.glider = (string)jo["glider"];
            this.takeoff = Parse.ParseNullableDateTimeOffset((string)jo["takeoff"], options.TimeZone);
            this.plane_landing = Parse.ParseNullableDateTimeOffset((string)jo["plane_landing"], options.TimeZone);
            this.glider_landing = Parse.ParseNullableDateTimeOffset((string)jo["glider_landing"], options.TimeZone);
            this.plane_time = Parse.ParseNullableTimeSpan((string)jo["plane_time"]);
            this.glider_time = Parse.ParseNullableTimeSpan((string)jo["glider_time"]);
            this.towplane_max_alt = (string)jo["towplane_max_alt"];
        }

        internal class Parse
        {
            /// <summary>
            /// Handling incomming formats of "" or "11:26:04"
            /// </summary>
            /// <param name="value"></param>
            /// <param name="timeZone"></param>
            /// <returns></returns>
            internal static DateTimeOffset? ParseNullableDateTimeOffset(string value, int timeZone)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                TimeSpan time;
                if (!TimeSpan.TryParse(value, out time))
                    return null;

                return new DateTimeOffset(time.Ticks, new TimeSpan(timeZone, 0, 0));
            }

            /// <summary>
            /// Handling incomming formats of "" or "00h05m52s"
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            internal static TimeSpan? ParseNullableTimeSpan(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                TimeSpan time;
                if (!TimeSpan.TryParseExact(value, @"hh\hmm\mss\s", null, out time))
                    return null;

                return time;
            }
        }

        public Guid FlightId { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public string airfield { get; set; }
        public string alt_setting { get; set; }
        public string unit { get; set; }
        public string timezone { get; set; }
        public string plane { get; set; }
        public string glider { get; set; }
        public DateTimeOffset? takeoff { get; set; }
        public DateTimeOffset? plane_landing { get; set; }
        public DateTimeOffset? glider_landing { get; set; }
        public TimeSpan? plane_time { get; set; }
        public TimeSpan? glider_time { get; set; }
        public string towplane_max_alt { get; set; }
    }
}
