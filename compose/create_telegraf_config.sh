#!/bin/env sh

if [[ -z "${INFLUX_ADMIN_TOKEN}" ]]; then
  echo "You need to set the INFLUX_ADMIN_TOKEN."
  exit 1
fi

if [[ -z "${INFLUX_BUCKET}" ]]; then
  echo "You need to set the INFLUX_BUCKET."
  exit 1
fi

if [[ -z "${INFLUX_ORG}" ]]; then
  echo "You need to set the INFLUX_ORG."
  exit 1
fi

cat <<EOF >telegraf.conf
[[outputs.influxdb_v2]]
  urls = ["http://influxdb:8086"]
  token = "$INFLUX_ADMIN_TOKEN"
  bucket = "$INFLUX_BUCKET"
  organization = "$INFLUX_ORG"

[[outputs.file]]
  files = ["stdout", "/tmp/metrics.out"]

[[inputs.mqtt_consumer]]
  servers = ["tcp://mosquitto:1883"]
  topics = [
    "rssi_topic/#"
  ]
EOF