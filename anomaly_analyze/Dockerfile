FROM python:3

WORKDIR /usr/src/app
COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY anomaly_script.py .

CMD [ "python", "./anomaly_script.py" ]
