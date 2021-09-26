# Description
- This folders contains models used to predict future rssi values and based on it predict possible discruption code if it exists
- The dataset is around 830000 datapoints for rssi data, in which it coinside with 1182 data points in disruption code.

# Predicting Rssi
- since rssi data points is a huge values we resampled a from it a sorted Datetime values around 30000 points, a build an LSTM model to predict future trends in rssi sensor readings.

# Predicting Disruption
- Since There is a highly imbalance dataset 1182 points contains code, while other (82000 is) is normal events.
- We downsampled the normal events to match number of rare-events, then we build a binary classifier to predict based on rssi values if there is a disruption or not. the model accurayc was around 73%

- Then we build a multi-class event preictor, and validated the model based on binary classifier. At first we sampled from normal events (No disruption a data points of size 1182), then for disruption code, we applied upsampling techniques to have a balnced class distribution so that the model could learn all classes. We used and Event based predictor to predict possible discruption code if exist based on rssi values and we validated that event occur(disruption ) based on binary classifier. Since uplsampling sometimes changes nature of data.

- Now we have model that can predict ahead of time the values of rssi and based on it predict possible diruption if it exist.


# Visualization
1- Rssi values prediction
![Alt text](imgs/rssi.png?raw=true "Title")
2- Failure preciction
![Alt text](imgs/disruption.png?raw=true "Title")


