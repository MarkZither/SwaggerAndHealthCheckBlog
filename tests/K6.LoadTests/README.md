# K6 Load Tests

## Writing to InfluxDB

``` cmd
k6 run --out influxdb=http://localhost:8086/myk6db script.js
```

## Viewing the stats in Grafana

Import the dashboard https://grafana.com/grafana/dashboards/2587

![K6 Grafana Dashboard Screenshot](assets/K6_Grafana.png)