build: build_simulator build_config

build_simulator:
	cd traffic_simulator && docker build -t traffic_simulator .

build_config:
	cd compose && sh create_telegraf_config.sh

compose_up: build_config
	cd compose && docker-compose up -d

compose_down:
	cd compose && docker-compose down

run_simulator:
	docker run -v ${PWD}/sample_static_data:/app/static_data --network=compose_iot --rm traffic_simulator cargo run --release -- --rssi-data static_data/rssi.csv --broker=tcp://mosquitto:1883 --entry-limit=10 --sleep-period-ms=1000

clean:
	rm compose/telegraf.conf

