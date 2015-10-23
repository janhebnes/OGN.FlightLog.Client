using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OGN.FlightLog.Client.Test
{
    [TestClass]
    public class OGNFlightLogClientTest
    {
        [TestMethod]
        public void StaticClientLiveRequest()
        {
            var options = new Client.Options("EHDL", new DateTime(2015, 05, 30));
            var flights = Client.GetFlights(options);
            Assert.IsNull(flights);

            flights = Client.GetFlights(options, false);
            Assert.IsTrue(flights.Count == 20);
        }

        [TestMethod]
        public void InstanceClientLiveRequest()
        {
            var client = new Client("EHDL", 2);
            var flights = client.GetFlights(new DateTime(2015, 05, 30));
            Assert.IsNull(flights);

            flights = client.GetFlights(new DateTime(2015, 05, 30), false);
            Assert.IsTrue(flights.Count == 20);
        }
    }
}
