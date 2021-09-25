FROM rust:1.55

RUN apt-get update && apt-get install -y cmake

WORKDIR /app
COPY . /app

RUN cargo build --release

ENV RUST_LOG=info
CMD cargo run --release 
