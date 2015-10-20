using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace OGN_FlightLog_Dataminer
{
    public class Flight
    {
        public Guid FlightId { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public string Airfield { get; set; }
        [DataType(DataType.Time)]
        public DateTime? takeoff { get; set; }
        [DataType(DataType.Time)]
        public DateTime? plane_landing { get; set; }
        [DataType(DataType.Time)]
        public DateTime? glider_landing { get; set; }
        [DataType(DataType.Time)]
        public DateTime? plane_time { get; set; }
        [DataType(DataType.Time)]
        public DateTime? glider_time { get; set; }
        public int? towplane_max_alt { get; set; }
    }
}
