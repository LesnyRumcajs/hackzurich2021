import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import warnings
from sklearn.ensemble import IsolationForest
warnings.filterwarnings("ignore")

df = pd.read_csv('/Users/aarsh/Downloads/rssi.csv',usecols=['DateTime','A2_RSSI'],parse_dates=['DateTime'])
#getting the data(1000 points)

def anomaly(df):
    outliers_fraction = 0.06
    X1 = df['A2_RSSI'].values.reshape(-1,1)
    model = IsolationForest(contamination=outliers_fraction)
    model.fit(X1)
    df['anomaly'] = pd.Series(model.predict(X1))
    return df
#function returns the dataframe with anomaly column(-1 for anomalies)