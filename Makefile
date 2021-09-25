build:
	cd traffic_simulator && docker build -t traffic_simulator .

run_simulator:
	docker run -v ${PWD}/sample_static_data:/app/static_data --network=docker_example_iot --rm traffic_simulator cargo run --release -- --rssi-data static_data/rssi.csv --broker=tcp://mosquitto:1883 --entry-limit=10 --sleep-period-ms=1000

