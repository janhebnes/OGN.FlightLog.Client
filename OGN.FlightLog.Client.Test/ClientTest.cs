using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OGN.FlightLog.Client.Test
{
    [TestClass]
    public class OGNFlightLogClientTest
    {
        [TestInitialize]
        public void init()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
        }

        [TestMethod]
        public void StaticClientStoredRequest()
        {
            var options = new Client.Options("EHDL", new DateTime(2015, 05, 30));
            var flights = Client.GetFlights(options);
            Assert.IsTrue(flights.Count == 20);
        }

        [TestMethod]
        public void StaticClientLiveRequest()
        {
            var options = new Client.Options("EHDL", new DateTime(2015, 05, 30));
            var flights = Client.GetFlights(options);
            Assert.IsNotNull(flights);
            Assert.IsTrue(flights.Count == 20);

            flights = Client.GetFlights(options, false);
            Assert.IsTrue(flights.Count == 20);
        }

        [TestMethod]
        public void InstanceClientLiveRequest()
        {
            var client = new Client("EHDL", 2);
            var flights = client.GetFlights(new DateTime(2015, 05, 30));
            Assert.IsNotNull(flights);
            Assert.IsTrue(flights.Count == 20);

            flights = client.GetFlights(new DateTime(2015, 05, 30), false);
            Assert.IsTrue(flights.Count == 20);
        }

        [TestMethod]
        public void StaticClientLiveInvalidAirportRequest()
        {
            try
            {
                var options = new Client.Options("EKKS", new DateTime(2015, 05, 30));
                var flights = Client.GetFlights(options, false);
                Assert.Fail();
            }
            catch (Client.InvalidAirportException iae)
            {
                Assert.IsNotNull(iae);
            }
            
        }
    }
}
