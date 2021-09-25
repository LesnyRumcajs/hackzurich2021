import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import warnings
import os
import time
from sklearn.ensemble import IsolationForest

from datetime import datetime
from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS

warnings.filterwarnings("ignore")

token      = os.environ.get("INFLUXDB_TOKEN")
org        = os.environ.get("INFLUXDB_ORGANIZATION")
bucket     = os.environ.get("INFLUXDB_BUCKET")
client = InfluxDBClient(url="http://influxdb:8086", token=token)

measurement = 'a2_rssi'
field = 'signalStrength'

def getDataPoints(count):
    query = f'from(bucket: "{bucket}") |> range(start: -{count}s) |> filter(fn: (r) => r._measurement == "{measurement}" and r._field == "{field}")'
    data = { 'timestamp': [], field: [] }

    for record in client.query_api().query(query, org=org)[0].records:
        data['timestamp'].append(record.values.get('_time'))
        data[field].append(record.values.get('_value'))

    return data;

def updateDataPoints(df):
    write_api = client.write_api(write_options=SYNCHRONOUS)
    sequence = []

    for index, row in df.iterrows():
        anomaly = row['anomaly']
        ticks = row['timestamp'].value
        sequence.append(f'{measurement} anomaly={anomaly} {ticks}')

    write_api.write(bucket, org, sequence)

def anomaly(df):
    outliers_fraction = 0.06
    X1 = df[field].values.reshape(-1,1)
    model = IsolationForest(contamination=outliers_fraction)
    model.fit(X1)
    df['anomaly'] = pd.Series(model.predict(X1))
    return df


def main():    
    data = getDataPoints(1000)
    df = pd.DataFrame.from_dict(data)
    result = anomaly(df)
    updateDataPoints(result)

while True:
    main()
    time.sleep(10)
