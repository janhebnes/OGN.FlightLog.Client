# OGN.FlightLog.Client

Gives a .net interface for getting the results from the OGN FlightLog
http://live.glidernet.org/flightlog/index.php?a=EHDL&s=QFE&u=M&z=2&p=&d=30052015&j 

```c#
var options = new Client.Options("EHDL", new DateTime(2015, 05, 30));
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

Please notice that Open Glider Network has requested low levels of requests to the Flightlog interface. The service has not been optimized for large batches. So please limit the live requests to e.g. 20 min intervals and if you request all historical information for a location put in some latency between each request to not stress the OGN flightlog service. 
