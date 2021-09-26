# 🇨🇭 HackZurich2021 🚋 Siemens challenge 🚅

![CI](https://github.com/LesnyRumcajs/hackzurich2021/workflows/CI/badge.svg)

## About
The system here was written with a noble goal to help the Swiss trains be even less often late. So hard to imagine! Even harder to implement! On top of that so little time to achieve this! Still, we strived to follow some best industry practices. Hopefully there's no secret key laying around in the repo!

## Development

### Prerequisites
Linux with Docker (or MacOS with Docker for MacOS). Should work on WSL2 without too much hassle as well. Nothing else!

### Data
Put the extracted Siemens Challenge static CSV data ([link!](http://hackzurich.siemens.cool/#/)) to directory `static_data` (too large to upload here). It should look like this:
```
❯ ls static_data/
disruptions.csv  events.csv  Mapping_Events_Disruptions.csv  rssi.csv  velocities.csv
```

### Setup
You need the environmental variables below set up, e.g.
```
export INFLUX_BUCKET=hackzurich
export INFLUX_ORG=HackZurich
export INFLUX_USERNAME=admin
export INFLUX_PASSWORD=<YOURPASSWORD>
export INFLUX_ADMIN_TOKEN=<YOURTOKEN>
```

Build the traffic generator and system configuration:
```
make build
```
Bring the system up:
```
make compose_up
```
Run the traffic generator with for a short time to check if all is good:
```
make run_simulator
```

Other recipes can be found in the [Makefile](https://github.com/LesnyRumcajs/hackzurich2021/blob/main/Makefile).

### Take a look!

You access the heart of the system the *InfluxDB* [http://localhost:8086](http://localhost:8086). Be sure to play around here!

The frontend of the system is available under [http://localhost:8080](http://localhost:8080). A fancy map awaits you there!

