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
        public FlightLogContext() : base("OGN.FlightLog")
        {
            Database.SetInitializer<FlightLogContext>(new MigrateDatabaseToLatestVersion<FlightLogContext, Migrations.FlightLogContext.Configuration>());
        }
        
        public DbSet<Flight> Flights { get; set; }
    }


  

}
