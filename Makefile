# Builds the entire platform
build: build_simulator build_frontend build_config

# Builds the traffic simulator
build_simulator:
	cd traffic_simulator && docker build -t traffic_simulator .

build_frontend:
	cd hz-siemens-fe && docker build -t hz-siemens-fe .

build_webapi:
		cd webApi && docker build -t webapi -f HackZurich2021.WebApi/Dockerfile .

# Generates configuration for the system to mount
build_config:
	sh create_config_files.sh

# Builds up the system
compose_up: build_config
	cd compose && \
		docker-compose up -d && \
		echo "Applying template..." && \
		sleep 30 && \
		docker exec compose_influxdb_1 /usr/local/bin/influx apply --org=HackZurich -f /var/opt/influxdb_template --quiet --force=true

# Tears down the system
compose_down:
	cd compose && docker-compose down

# Runs a sample traffic simulation
run_simulator:
	docker run -v ${PWD}/sample_static_data:/app/static_data --network=compose_iot --rm traffic_simulator cargo run --release -- --rssi-data static_data/rssi.csv --broker=tcp://mosquitto:1883 --entry-limit=0 --sleep-period-ms=100

# Removes temporary assets
clean:
	rm compose/telegraf.conf

