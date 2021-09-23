# Setup docker environment

## Install docker
Just have a look into the docker documentation and you will find a guide suitable for your system. I would recommend to install portainer as it's simplifies the management of the docker environment.
- https://docs.docker.com/engine/install/
- https://support.portainer.io/kb/article/47-how-to-install-portainer/

## Run compose file/stack
### Using CLI
Just navigate to the folder where you placed the 'docker-compose.yml' and the 'telegraf.conf' and run the following command(s).
- Start
> 	 docker-compose up -d
- Stop
> 	 docker-compose stop

### Using portainer
You can load the .yml into the portainer and manage it there. Have a look into the documentation at: https://www.portainer.io/blog/stacks-docker-compose-the-portainer-way

## Check the environment
### Running containers
Now there should be 4 containers (5 if you're using portainer) running:
- telegraf
- grafana
- mosquitto
- influxdb

You can check this via CLI using the following command:
> docker ps

Using portainer just navigate to the 'Containers' dashboard.

### Web dashboards
Some of the containers also have a web dashboard the manage the application. You have to call them to initialize the instances, e.g. set a initial organization and bucket for InfluxDB. 
- influxdb → http://localhost:8086/
- grafana → http://localhost:3000/

### MQTT
The mosquitto container provides a endpoint for the broker at 'mqtt://localhost:1883'. Use any MQTT Client to check the connection, e.g. https://mqtt-explorer.com/.

------------

# Send data to the InfluxDB

## Using Telegraf/MQTT
### Generate/Lookup token and set environments entries
Navigate to the InfluxDB Dashboard at 'http://localhost:8086/' → Data → Tokens. Create a new token or use an existing one. Copy the hash value and replace the '$TOKEN' tag in the 'telegraf.conf' file´. Also replace the '$BUCKET' and '$ORGANIZATION' tags with the ones you created during setup.

After you saved the configuration file you have to restart the telegraf container apply it.

### Send data using MQTT Client
Connect to the MQTT Broker at 'mqtt://localhost:1883' and publish data to the predefined topic 'sensor' use the default InfluxDB format: 'measurement value1=42 value2=666'

## Using Python Client

### Generate client code using the InfluxDB dashboard
Navigate to the InfluxDB Dashboard at 'http://localhost:8086/' → Data → Sources → Python. Just select the token and bucket you want to use, copy the generated code and paste it into your .py file. Here's a simple example:
```python
from datetime import datetime

from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS

token = "8sxuX_VWq7N_X1VMznFe4kusteaaPLRPYVStLFul0hb6ryi7UpHQSfJYG2GdtjFjdcvGib5mWTtyVdNOE8z3CQ=="
org = "influx"
bucket = "influx"

client = InfluxDBClient(url="http://localhost:8086", token=token)

write_api = client.write_api(write_options=SYNCHRONOUS)

data = "mem,host=host1 used_percent=23.43234543"
write_api.write(bucket, org, data)
```







