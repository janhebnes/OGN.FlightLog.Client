using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace OGN.FlightLog.Client.Models
{
    [Table("logbook", Schema = "ktrax")]
    public class Flight
    {
        public static string Header = "DATE,SEQ_NR,ID,CALLSIGN,COMPETITION_NUMBER,TYPE,DETAILED_TYPE,CREW1_NAME,CREW1_NR,RESERVED_1,CREW2_NAME,CREW2_NR,RESERVED_2,TKOF_TIME,TKOF_AP,TKOF_RWY,RESERVED_3,LDG_TIME,LDG_AP,LDG_RWY,LDG_TURN,MAX_ALT,VERTICAL_SPEED,INITIAL_CLIMBRATE,AVERAGE_CLIMB_RATE,FLIGHT_TIME,DAY_DIFFERENCE,LAUNCH_METHOD,TOW_ID,TOW_CALLSIGN,TOW_COMPETITION_NUMBER,TOW_SEQUENCE_NUMBER";
        public enum Columns
        {
            //DATE = 0, //2021-04-27
            SEQ_NR = 1, //161952234701
            ID = 2, //flarm:DDDC94
            CALLSIGN = 3, //OY-XMD
            COMPETITION_NUMBER = 4, //MD
            TYPE = 5, //1
            DETAILED_TYPE = 6, //LS-4
            //CREW1_NAME = 7,
            CREW1_NR = 8,
            //RESERVED_1 = 9,
            //CREW2_NAME = 10,
            CREW2_NR = 11,
            //RESERVED_2 = 12,
            TKOF_TIME = 13, //13:19:01
            TKOF_AP = 14, //EKAS
            TKOF_RWY = 15, //29
            //RESERVED_3 = 16, //0
            LDG_TIME = 17, //13:23:38
            LDG_AP = 18, //EKAS
            LDG_RWY = 19, //27
            LDG_TURN = 20, //1.6
            MAX_ALT = 21, //270
            //VERTICAL_SPEED = 22, //0
            INITIAL_CLIMBRATE = 23, //11.7043
            AVERAGE_CLIMB_RATE = 24, //9.79
            FLIGHT_TIME = 25, //0:04:37
            DAY_DIFFERENCE = 26, //0
            LAUNCH_METHOD = 27, //W
            TOW_ID = 28,
            TOW_CALLSIGN = 29,
            TOW_COMPETITION_NUMBER = 30,
            TOW_SEQUENCE_NUMBER = 31
        }

        public Flight()
        {
            // Used only when entity framework initializes objects
            State = EntityState.Unchanged;
        }

        public Flight(Client.Options options, int row)
        {
            // E.g. "EHDL30052015QFEz2m"
            this.dataset = options.GetDatasetIdentifier();
            this.row = row;

            // Flight id based on parameters + row in datasource e.g. "EHDL30052015z2metric" + "1"
            this.ID = this.dataset + this.row.ToString();

            // FROM options
            //   "date": "30052015",
            //   "airfield": "EHDL",
            //   "unit": "metric",
            this.Date = options.Date.Date;
            this.airfield = options.AirfieldParameter;
            this.unit = options.UnitParameter;
            this.timezone = options.TimeZoneParameter;
        }

        public Flight(Client.Options options, int row, string line) : this(options, row)
        {
            string[] data = line.Split(',');

            if (data.Length < (int) (Columns.TOW_SEQUENCE_NUMBER + 1))
                return;

            this.seq_nr = Parse.Bigint(data[(int)Columns.SEQ_NR]); // 8-bit int 
            this.identifier = data[(int)Columns.ID]; // flarm:xxx
            this.callsign = data[(int)Columns.CALLSIGN];
            this.competition_number = data[(int)Columns.COMPETITION_NUMBER];
            this.plane_type = data[(int)Columns.TYPE];
            this.detailed_plane_type = data[(int)Columns.DETAILED_TYPE];
            this.crew1 = data[(int)Columns.CREW1_NR];
            this.crew2 = data[(int)Columns.CREW2_NR];
            this.tkof_time = Parse.DateTimeOffset(data[(int)Columns.TKOF_TIME], options.TimeZone);
            this.tkof_ap = data[(int)Columns.TKOF_AP];
            this.tkof_rwy = Parse.Int(data[(int)Columns.TKOF_RWY]);
            this.ldg_time = Parse.DateTimeOffset(data[(int)Columns.LDG_TIME], options.TimeZone);
            this.ldg_ap = data[(int)Columns.LDG_AP];
            this.ldg_rwy = Parse.Int(data[(int)Columns.LDG_RWY]);
            this.ldg_turn = Parse.Decimal(data[(int)Columns.LDG_TURN]);
            this.max_alt = Parse.Int(data[(int)Columns.MAX_ALT]);
            this.average_climb_rate = Parse.Decimal(data[(int)Columns.AVERAGE_CLIMB_RATE]);
            this.flight_time = Parse.TimeSpan(data[(int)Columns.FLIGHT_TIME]);
            this.day_difference = Parse.Int(data[(int)Columns.DAY_DIFFERENCE]);
            this.launch_method = data[(int)Columns.LAUNCH_METHOD];
            this.initial_climbrate = Parse.Decimal(data[(int)Columns.INITIAL_CLIMBRATE]);
            this.tow_identifier = data[(int)Columns.TOW_ID];
            this.tow_callsign = data[(int)Columns.TOW_CALLSIGN];
            this.tow_competition_number = data[(int)Columns.TOW_COMPETITION_NUMBER];
            this.tow_sequence_number = data[(int)Columns.TOW_SEQUENCE_NUMBER]; 
        }

        internal class Parse
        {
            /// <summary>
            /// Handling incomming formats of "" or "11:26:04" and taking care of adding the timeZone information into the entry
            /// </summary>
            /// <param name="value"></param>
            /// <param name="timeZone"></param>
            /// <returns></returns>
            internal static DateTimeOffset? DateTimeOffset(string value, int timeZone)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (!System.TimeSpan.TryParse(value, out var time))
                {
                    if (!value.EndsWith("XX"))
                        return null;

                    var first = value.Split(':').First();
                    
                    if (!int.TryParse(first, out var hour))
                        return null;

                    return new DateTimeOffset(new TimeSpan(hour, 0, 0).Ticks, new TimeSpan(timeZone, 0, 0));
                }

                return new DateTimeOffset(time.Ticks, new TimeSpan(timeZone, 0, 0));
            }

            /// <summary>
            /// Handling incomming formats of "" or "00:05" for hours and minutes
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            internal static TimeSpan? TimeSpan(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (!DateTime.TryParseExact(value, "H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    if (!value.EndsWith("XX"))
                        return null;
                    
                    var first = value.Split(':').First();

                    if (!int.TryParse(first, out var hour))
                        return null;
                    
                    return new TimeSpan(hour, 0, 0);
                }
                return dt.TimeOfDay;
            }

            internal static Decimal? Decimal(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal numeric))
                    return numeric;

                return null;
            }

            internal static int? Int(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (int.TryParse(value, out int numeric))
                    return numeric;

                return null;
            }

            internal static Int64? Bigint(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (Int64.TryParse(value, out Int64 numeric))
                    return numeric;

                return null;
            }
        }

        public string dataset { get; set; }
        public int row { get; set; }

        [Key]
        public string ID { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public string airfield { get; set; }
        public string unit { get; set; }
        public string timezone { get; set; }

        public Int64? seq_nr { get; set; }
        public string identifier { get; set; }
        public string callsign { get; set; }
        public string competition_number { get; set; }
        public string plane_type { get; set; }
        public string detailed_plane_type { get; set; }
        public string crew1 { get; set; }
        public string crew2 { get; set; }
        public DateTimeOffset? tkof_time { get; set; }
        public string tkof_ap { get; set; }
        public int? tkof_rwy { get; set; }
        public DateTimeOffset? ldg_time { get; set; }
        public string ldg_ap { get; set; }
        public int? ldg_rwy { get; set; }
        public decimal? ldg_turn { get; set; }
        public int? max_alt { get; set; }
        public decimal? average_climb_rate { get; set; }
        public TimeSpan? flight_time { get; set; }
        public int? day_difference { get; set; }
        public string launch_method { get; set; }
        public decimal? initial_climbrate { get; set; }
        public string tow_identifier { get; set; }
        public string tow_callsign { get; set; }
        public string tow_competition_number { get; set; }
        public string tow_sequence_number { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public EntityState State { get; set; }

        public override string ToString() => $"{callsign} {tkof_time:HH\\:mm} {flight_time:hh\\:mm}";
    }
}
