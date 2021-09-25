# hackzurich2021

Put the extracted static CSV data to directory `static_data` (too large to upload here). It should look like this:
```
❯ ls static_data/
disruptions.csv  events.csv  Mapping_Events_Disruptions.csv  rssi.csv  velocities.csv
```

Build the traffic generator:
```
make build

```

Run the traffic generator:
```
make run_simulator
```
