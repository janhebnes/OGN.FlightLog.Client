using OGN.FlightLog.Client;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static OGN.FlightLog.Client.Client;

namespace OGN.FlightLog.Client.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestInitialize]
        public void init()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
        }

        [TestMethod]
        public void StaticClientStoredRequest()
        {
            var options = new Client.Options("EKKS", new DateTime(2019, 04, 21));
            var flights = Client.GetFlights(options);
            Assert.IsTrue(flights.Count == 36);
        }

        [TestMethod]
        public void StaticClientLiveRequest()
        {
            var options = new Client.Options("EKKS", new DateTime(2019, 04, 21));
            var flights = Client.GetFlights(options);
            Assert.IsNotNull(flights);
            Assert.IsTrue(flights.Count == 36);

            flights = Client.GetFlights(options, false);
            Assert.IsTrue(flights.Count == 36);
        }

        [TestMethod]
        public void InstanceClientLiveRequest()
        {
            var client = new Client("EKKS", 2);
            var flights = client.GetFlights(new DateTime(2019, 04, 21));
            Assert.IsNotNull(flights);
            Assert.IsTrue(flights.Count == 36);

            flights = client.GetFlights(new DateTime(2019, 04, 21), false);
            Assert.IsTrue(flights.Count == 36);
        }

        [TestMethod]
        public void BasicDownloadTest()
        {
            var options = new Client.Options("EKKS", 2, new DateTime(2019, 04, 21));
            System.Net.WebClient client = new Client.WebClientWithTimeout(options.Timeout);
            var result = client.DownloadString(options.ToCsvDownloadAddress());

            Assert.IsTrue(result.Contains("SUM_DALT,14846.2"));
        }

        [TestMethod]
        public void BasicFlightParsingUsingSampleCsvTest()
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.csv");

            Options options = new Options("EKKS", 2, new DateTime(2019, 4, 21));

            var result = new List<Models.Flight>();
            int row = 0;
            foreach (string line in System.IO.File.ReadAllLines(samplePath))
            {
                bool IsMetaDataFooterSection = line.StartsWith(string.Intern("BEGIN_DATE,") + options.DateParameter);
                if (IsMetaDataFooterSection)
                    break;

                if (row++ == 0) continue;

                result.Add(new Models.Flight(options, row++, line));
            }

            Assert.IsTrue(result[0].flight_time == new TimeSpan(0, 6, 0));
            Assert.IsTrue(result[0].average_climb_rate == 9.22m);
            Assert.IsTrue(result.Count == 36);

            var datasetIdentifier = options.GetDatasetIdentifier();
            using (var db = new Models.FlightLogContext())
            {
                var flights = db.Logbook.Where(f => f.dataset == datasetIdentifier);
                if (flights.Any())
                {
                    foreach (var flight in flights)
                    {
                        db.Logbook.Remove(flight);
                    }
                    db.SaveChanges();
                    flights = db.Logbook.Where(f => f.dataset == datasetIdentifier);
                }
                if (!flights.Any())
                {
                    db.Logbook.AddRange(result);
                    db.SaveChanges();
                }
            }

            using (var db = new Models.FlightLogContext())
            {
                var flights = db.Logbook.Where(f => f.dataset == datasetIdentifier);
                Assert.IsTrue(flights.Count() == 36);

                var dbSpecificResult = flights.ToList().First(d => d.ID == result[0].ID);
                Assert.IsTrue(dbSpecificResult.flight_time == new TimeSpan(0, 6, 0));
                Assert.IsTrue(dbSpecificResult.average_climb_rate == 9.22m);
                Assert.IsTrue(dbSpecificResult.flight_time == new TimeSpan(0, 6, 0));
                
                foreach (var dbResult in flights.ToList())
                {
                    var parseResult = result.First(r=>r.ID == dbResult.ID);
                    // Complete validation of fields between database and parsed representation

                    Assert.AreEqual(parseResult.timezone, dbResult.timezone);
                    Assert.AreEqual(parseResult.airfield, dbResult.airfield);
                    Assert.AreEqual(parseResult.unit, dbResult.unit);
                    Assert.AreEqual(parseResult.Date, dbResult.Date);

                    Assert.AreEqual(parseResult.identifier, dbResult.identifier);
                    Assert.AreEqual(parseResult.callsign, dbResult.callsign);
                    Assert.AreEqual(parseResult.competition_number, dbResult.competition_number);
                    Assert.AreEqual(parseResult.plane_type, dbResult.plane_type);
                    Assert.AreEqual(parseResult.detailed_plane_type, dbResult.detailed_plane_type);
                    Assert.AreEqual(parseResult.crew1, dbResult.crew1);
                    Assert.AreEqual(parseResult.crew2, dbResult.crew2);
                    Assert.AreEqual(parseResult.tkof_time, dbResult.tkof_time);
                    Assert.AreEqual(parseResult.tkof_ap, dbResult.tkof_ap);
                    Assert.AreEqual(parseResult.tkof_rwy, dbResult.tkof_rwy);
                    Assert.AreEqual(parseResult.ldg_time, dbResult.ldg_time);
                    Assert.AreEqual(parseResult.ldg_ap, dbResult.ldg_ap);
                    Assert.AreEqual(parseResult.ldg_rwy, dbResult.ldg_rwy);
                    Assert.AreEqual(parseResult.ldg_turn, dbResult.ldg_turn);
                    Assert.AreEqual(parseResult.max_alt, dbResult.max_alt);
                    Assert.AreEqual(parseResult.average_climb_rate, dbResult.average_climb_rate);
                    Assert.AreEqual(parseResult.flight_time, dbResult.flight_time);
                    Assert.AreEqual(parseResult.day_difference, dbResult.day_difference);
                    Assert.AreEqual(parseResult.launch_method, dbResult.launch_method);
                    Assert.AreEqual(parseResult.initial_climbrate, dbResult.initial_climbrate);
                    Assert.AreEqual(parseResult.tow_identifier, dbResult.tow_identifier);
                    Assert.AreEqual(parseResult.tow_callsign, dbResult.tow_callsign);
                    Assert.AreEqual(parseResult.tow_competition_number, dbResult.tow_competition_number);
                    Assert.AreEqual(parseResult.tow_sequence_number, dbResult.tow_sequence_number);
                    Assert.AreEqual(parseResult.seq_nr, dbResult.seq_nr);
                }
            }
        }

        #region sync feature is put on ice
        ////[TestMethod]
        ////public void GetChangeTrackedFlightsTest_unchanged_added_modified_deleted_state()
        ////{
        ////    var options = new Client.Options("EHDL", new DateTime(2015, 05, 30));
        ////    var flights = Client.GetFlights(options);
        ////    Assert.IsNotNull(flights);
        ////    Assert.IsTrue(flights.Count == 20);

        ////    var UnchangedFlights = flights.Exists(f => f.State != Models.EntityState.Unchanged);
        ////    Assert.IsTrue(UnchangedFlights);

        ////    // ADD
        ////    flights.Add(new Models.Flight(options, 999));

        ////    var changeTrackedflights = Client.GetChangeTrackedFlights(options, flights);
        ////    Assert.IsNotNull(changeTrackedflights);
        ////    Assert.IsTrue(changeTrackedflights.Count == 21);

        ////    var AddedFlightCount = changeTrackedflights.Count(f => f.State == Models.EntityState.Added);
        ////    Assert.IsTrue(AddedFlightCount == 1);

        ////    // MODIFY
        ////    //TODO: WRITE TEST
        ////    //TODO: WRITE TEST
        ////    //TODO: WRITE TEST
        ////    //TODO: WRITE TEST

        ////    // DELETE
        ////    changeTrackedflights.RemoveAll(f => f.row == 999);

        ////    var revertedChangeTrackedflights = Client.GetChangeTrackedFlights(options, changeTrackedflights);
        ////    Assert.IsNotNull(revertedChangeTrackedflights);
        ////    Assert.IsTrue(revertedChangeTrackedflights.Count == 21);

        ////    var Unchanged = revertedChangeTrackedflights.Count(f => f.State == Models.EntityState.Unchanged);
        ////    Assert.IsTrue(Unchanged == 20);

        ////    var deleted = revertedChangeTrackedflights.Count(f => f.State == Models.EntityState.Deleted);
        ////    Assert.IsTrue(deleted == 1);

        ////    // VALIDATE EMPTY ON CLEAN FETCH
        ////    var cleanflights = Client.GetFlights(options);
        ////    Assert.IsNotNull(cleanflights);
        ////    Assert.IsTrue(cleanflights.Count == 20);
        ////    var UnchangedCleanFlights = cleanflights.Count(f => f.State == Models.EntityState.Unchanged);
        ////    Assert.IsTrue(UnchangedCleanFlights == 20);
        ////}

        #endregion
    }
}

