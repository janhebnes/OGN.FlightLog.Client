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
        private static FlightLogContext instance = null;
        private static readonly object padlock = new object();

        public FlightLogContext() : base("OGN.FlightLog")
        {
        }

        /// <summary>
        /// Thread-Safe Singleton Instance
        /// </summary>
        public static FlightLogContext Instance
        {
            // TODO: Do we create a new Db Context for Ogn log data in owin or do we singleton ?
            //  app.CreatePerOwinContext(ApplicationDbContext.Create);
            //  context.Get<ApplicationDbContext>() that allows for a single instance to be used all over... 
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FlightLogContext();
                    }
                    return instance;
                }
            }
        }

        static FlightLogContext()
        {
            Database.SetInitializer<FlightLogContext>(new MigrateDatabaseToLatestVersion<FlightLogContext, Migrations.FlightLogContext.Configuration>());
        }

        public static FlightLogContext Create()
        {
            return new FlightLogContext();
        }

        public DbSet<Flight> Flights { get; set; }
    }


  

}
