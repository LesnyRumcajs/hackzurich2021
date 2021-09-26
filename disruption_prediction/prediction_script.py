import pandas as pd
import numpy as np
import tensorflow as tf

from datetime import datetime
from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS

from keras.models import load_model
import os
import time

token      = os.environ.get("INFLUXDB_TOKEN")
org        = os.environ.get("INFLUXDB_ORGANIZATION")
bucket     = os.environ.get("INFLUXDB_BUCKET")
client = InfluxDBClient(url="http://localhost:8086", token=token)

measurement = 'a2_rssi'
field = 'signalStrength'
model_rssi=load_model('rssi_forcast.h5')
model_events=load_model('model_3_multi.h5')

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
        forecast_date = row['forecasted_dates']
        event = row['disruption_event']
        rssi = row['forcasted_rssi']
        ticks = row['timestamp'].value
        sequence.append(f'disruption_event={event} forecasted_rssi={rssi} predicted_dates={forecast_date} {ticks}')

    write_api.write(bucket, org, sequence)

def annotate_event(out_predict):
  if out_predict==0:
    return 'No distruption'
  elif out_predict==1:
    return 'Hardwarefehler Bediengeraet'
  elif out_predict==2:
    return 'Hardwarefehler Verteiler'
  elif out_predict==3:
    return 'Position unverifiziert (Eichung falsch)'
  elif out_predict==4:
    return 'Keine Linienleitertelegramme empfangen'
  elif out_predict==5:
    return 'Zwangsbremse wurde aktiviert'
  elif out_predict==6:
    return 'Position unbekannt (ZSI0)'
  elif out_predict==7:
    return 'Stoerung: Zwangsbremse wurde aktiviert'

def predict(num_prediction, df, model_rssi,model_events):
    look_back=15
    prediction_list = df[-look_back:]
    disrupution_events=[]
    
    for _ in range(num_prediction):
        x = prediction_list[-look_back:]
        x = x.reshape((1, look_back, 1))
        x_2=prediction_list[-10:]
        x_2 = x_2.reshape((1, 10, 1))
        out = model_rssi.predict(x)[0][0]
        prediction_list = np.append(prediction_list, out)
        out = np.argmax(model_events.predict(x_2),axis=1)
        disrupution_events.append(annotate_event(out))
    prediction_list = prediction_list[look_back-1:]

    return prediction_list,disrupution_events

def predict_dates(num_prediction,df):
    last_date = df['DateTime'].values[-1]
    prediction_dates = pd.date_range(last_date, periods=num_prediction+1).tolist()
    return prediction_dates


def final_predictions(df):
    num_prediction = 30
    forecast_rssi,disrupution_forcasted = predict(num_prediction,df, model_rssi,model_events)
    forecast_dates = predict_dates(num_prediction,df)
    return forecast_rssi , forecast_dates , disrupution_forcasted

def main():  
    data = getDataPoints(15)
    df = pd.DataFrame.from_dict(data)
    forecasted_rssi,forecast_dates, disruption_event = final_predictions(df)
    df['forecasted_rssi'] = pd.Series(forecasted_rssi)
    df['forecasted_dates'] = pd.Series(forecast_dates)
    df['disruption_event']= pd.Series(disruption_event)
    updateDataPoints(df)


while True:
    main()
    time.sleep(10)
