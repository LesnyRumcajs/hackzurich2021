use std::fs::File;
use std::io::BufRead;
use std::path::{Path, PathBuf};
use std::time::Duration;
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

    // configure mqtt client
    let mqtt_opts = paho_mqtt::CreateOptionsBuilder::new()
        .server_uri(config.broker)
        .client_id(config.client_id)
        .finalize();

    let cli = paho_mqtt::Client::new(mqtt_opts).unwrap_or_else(|err| {
        println!("Error creating the client: {:?}", err);
        process::exit(1);
    });

    // Define the set of options for the connection.
    let conn_opts = paho_mqtt::ConnectOptionsBuilder::new()
        .keep_alive_interval(Duration::from_secs(20))
        .clean_session(true)
        .finalize();

    // Connect and wait for it to complete or fail.
    if let Err(e) = cli.connect(conn_opts) {
        println!("Unable to connect:\n\t{:?}", e);
        process::exit(1);
    }
    if let Ok(lines) = read_lines(config.rssi_data) {
        for (count, line) in lines.enumerate() {
            if config.entry_limit != 0 && count as u32 >= config.entry_limit {
                break;
            }
            if let Ok(line) = line {
                let split = line.split(',').collect::<Vec<_>>();
                let datapoint = format!(
                    "rssi_data datetime={} positionnoleap={} lat={} long={} a1_total={} a1_valid={} a2_rssi={} a2_total={} a2_valid={}",
                    split[1], split[5], split[6], split[7], split[8], split[9], split[10], split[11], split[12]
                );

                println!("{}", datapoint);
                let msg = paho_mqtt::MessageBuilder::new()
                    .topic(&config.rssi_topic_name)
                    .payload(datapoint)
                    .qos(0)
                    .finalize();
                if let Err(e) = cli.publish(msg) {
                    println!("Error sending message! {:?}", e);
                }
            }
            std::thread::sleep(time::Duration::from_millis(config.sleep_period_ms.into()));
        }
    }

    cli.disconnect(None).unwrap();
}

fn read_lines<P>(filename: P) -> io::Result<io::Lines<io::BufReader<File>>>
where
    P: AsRef<Path>,
{
    let file = File::open(filename)?;
    Ok(io::BufReader::new(file).lines())
}
