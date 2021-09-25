# Builds the entire platform
build: build_simulator build_config

# Builds the traffic simulator
build_simulator:
	cd traffic_simulator && docker build -t traffic_simulator .

# Generates configuration for the system to mount
build_config:
	cd compose && sh create_telegraf_config.sh

# Builds up the system
compose_up: build_config
	cd compose && \
		docker-compose up -d && \
		echo "Applying template..." && \
		sleep 20 && \
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

