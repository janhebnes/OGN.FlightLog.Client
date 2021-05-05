# OGN.FlightLog.Client &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE)

Gives a .net interface for getting the results from the OGN (Open Glider Network) KTrax Logbook FlightLog 
at https://ktrax.kisstech.ch/logbook/ based on http://live.glidernet.org/


```c#
var options = new Client.Options("EKKS", new DateTime(2019, 04, 21));
var flights = Client.GetFlights(options);
```

## Install
```
PM > Install-Package OGN.FlightLog.Client
```
https://www.nuget.org/packages/OGN.FlightLog.Client

## Cached

Results for current date are always fetched live and results from prior dates are fetched once and stored in local entity framework created database storage. 

```
<connectionStrings>
    <add name="OGN.FlightLog" connectionString="Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\OGN.FlightLog.mdf;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient"/>
</connectionStrings>
```

## Notice 

From 2020 the ktrax services have become subscription based https://ktrax.kisstech.ch/contribute 

Note that Open Glider Network and KTrax Logbook has requested low levels of requests to the Flightlog interface. The service has not been optimized for large batches. So please limit the live requests to e.g. 20 min intervals and if you request all historical information for a location put in some latency between each request to not stress the flightlog service. 

