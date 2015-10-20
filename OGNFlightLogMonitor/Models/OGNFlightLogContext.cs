using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace OGN_FlightLog_Dataminer.Models
{
    public class OGNFlightLogContext : DbContext
    {
        public OGNFlightLogContext() : base("OGNFlightLog")
        {
        }

        public DbSet<Flight> Flights { get; set; }
    }

}
