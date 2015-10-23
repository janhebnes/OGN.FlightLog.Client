using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace OGNFlightLogClient.Models
{
    public class OGNFlightLogContext : DbContext
    {
        public OGNFlightLogContext() : base("OGNFlightLog")
        {
        }

        static OGNFlightLogContext()
        {
            //Database.SetInitializer<OGNFlightLogContext>(new MigrateDatabaseToLatestVersion<OGNFlightLogContext, Migrations.OGNFlightLogContext.Configuration>());
        }

        public static OGNFlightLogContext Create()
        {
            return new OGNFlightLogContext();
        }

        public DbSet<Flight> Flights { get; set; }
    }


  

}
