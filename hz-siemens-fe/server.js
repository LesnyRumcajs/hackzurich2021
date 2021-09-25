
var express = require("express");
var bodyParser = require("body-parser");
var MongoClient = require('mongodb').MongoClient
const uri = "mongodb+srv://hz2021app:hzapp234!@cluster0.ivgev.mongodb.net/siemensdb?retryWrites=true&w=majority";
const client = new MongoClient(uri, { useNewUrlParser: true, useUnifiedTopology: true });
let value = { latitude: 47.3200, longitude: 8.0528 }
var app = express();
app.use(bodyParser.json());
var distDir = __dirname + "/dist/";


app.use('/',express.static(distDir));
var server = app.listen(process.env.PORT || 8080, function () {
    var port = server.address().port;
    console.log("App now running on port", port);
});
app.get("/api/health", function (req, res) {
    res.status(200).json({ status: "UP" });
});

app.post("/api/tableData", function (req, res) {
    if (req.body.start_time != undefined) {
        var start = req.body.start_time
        var end = req.body.end_time
        res.status(200).json({ data: "" });
    }
    else{
        res.status(400).json({data:""});
    }
});

app.get("/api/getLatLong", function (req, res) {
    res.status(200).json(value);
})

app.get("/api/getAlert", function (req, res) {
    client.connect(err )
    res.status(200).json({})
})

app.put("/api/sendAlert", function(req, res){
    client.connect(err => {
        const collection = client.db("test").collection("devices");
        client.close();
      });
    res.status(200).json({})
})