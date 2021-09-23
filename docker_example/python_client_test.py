from datetime import datetime

from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS

# You can generate a Token from the "Tokens Tab" in the UI
token = "8sxuX_VWq7N_X1VMznFe4kusteaaPLRPYVStLFul0hb6ryi7UpHQSfJYG2GdtjFjdcvGib5mWTtyVdNOE8z3CQ=="
org = "influx"
bucket = "influx"

client = InfluxDBClient(url="http://localhost:8086", token=token)

write_api = client.write_api(write_options=SYNCHRONOUS)

data = "mem,host=host1 used_percent=23.43234543"
write_api.write(bucket, org, data)