using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace OGN.FlightLog.Client.Models
{
    public class FlightLogContext : DbContext
    {
        public FlightLogContext() : base("OGNFlightLog")
        {
        }

        static FlightLogContext()
        {
            //Database.SetInitializer<OGNFlightLogContext>(new MigrateDatabaseToLatestVersion<OGNFlightLogContext, Migrations.OGNFlightLogContext.Configuration>());
        }

        public static FlightLogContext Create()
        {
            return new FlightLogContext();
        }

        public DbSet<Flight> Flights { get; set; }
    }


  

}
