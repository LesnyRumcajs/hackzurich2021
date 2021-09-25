use log::{error, info};
use paho_mqtt::Client;
use std::fs::File;
use std::io::BufRead;
use std::path::{Path, PathBuf};
use std::time::{Duration, SystemTime, UNIX_EPOCH};
use std::{io, process, time};
use structopt::StructOpt;

#[derive(Debug, StructOpt)]
struct Config {
    #[structopt(long, default_value = "0")]
    entry_limit: u32,
    #[structopt(parse(from_os_str), long)]
    rssi_data: PathBuf,
    #[structopt(long, default_value = "tcp://localhost:1883")]
    broker: String,
    #[structopt(long, default_value = "some_client_id")]
    client_id: String,
    #[structopt(long, default_value = "rssi_topic")]
    rssi_topic_name: String,
    #[structopt(long, default_value = "0")]
    sleep_period_ms: u32,
}

fn main() {
    let config = Config::from_args();
    env_logger::init();

    let cli = create_mqtt_client(&config);

    if let Ok(lines) = read_lines(config.rssi_data) {
        for (count, line) in lines.enumerate().skip(1) {
            if config.entry_limit != 0 && count as u32 >= config.entry_limit {
                break;
            }
            if let Ok(line) = line {
                for datapoint in format_data(line) {
                    publish_data(&config.rssi_topic_name, &cli, datapoint)
                }
            }
            std::thread::sleep(time::Duration::from_millis(config.sleep_period_ms.into()));
        }
    }

    cli.disconnect(None).unwrap();
}

fn create_mqtt_client(config: &Config) -> Client {
    // configure mqtt client
    let mqtt_opts = paho_mqtt::CreateOptionsBuilder::new()
        .server_uri(&config.broker)
        .client_id(&config.client_id)
        .finalize();

    let cli = paho_mqtt::Client::new(mqtt_opts).unwrap_or_else(|err| {
        error!("Error creating the client: {:?}", err);
        process::exit(1);
    });

    // Define the set of options for the connection.
    let conn_opts = paho_mqtt::ConnectOptionsBuilder::new()
        .keep_alive_interval(Duration::from_secs(20))
        .clean_session(true)
        .finalize();

    // Connect and wait for it to complete or fail.
    if let Err(e) = cli.connect(conn_opts) {
        error!("Unable to connect:\n\t{:?}", e);
        process::exit(1);
    }
    cli
}

fn format_data(line: String) -> Vec<String> {
    let split = line.split(',').collect::<Vec<_>>();
    let timestamp = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("Time went backwards!")
        .as_nanos();
    let datapoints = vec![
        format!(
            "a1_telegram,category=telegram total={}i,valid={}i {}",
            split[8], split[9], timestamp),
        format!(
            "a2_telegram,category=telegram total={}i,valid={}i {}",
            split[11], split[12], timestamp
        ),
        format!(
            "a2_rssi,category=rssi signalStrength={} {}",
            split[10], timestamp
        ),
        format!(
            "position,category=position areaNumber={}i,latitude={},longitude={},position={}i,positionNoLeap={}i {}",
            split[2], split[6], split[7], split[4], split[5], timestamp
        ),
        format!(
            "identification,category=identification recordId={}i,track={}i {}",
            split[0], split[3], timestamp
        )
    ];
    datapoints
}

fn publish_data(topic: &str, cli: &Client, datapoint: String) {
    info!("{}", datapoint);
    let msg = paho_mqtt::MessageBuilder::new()
        .topic(topic)
        .payload(datapoint)
        .qos(0)
        .finalize();
    if let Err(e) = cli.publish(msg) {
        error!("Error sending message! {:?}", e);
    }
}

fn read_lines<P>(filename: P) -> io::Result<io::Lines<io::BufReader<File>>>
where
    P: AsRef<Path>,
{
    let file = File::open(filename)?;
    Ok(io::BufReader::new(file).lines())
}
