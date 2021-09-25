import { Component, OnInit } from '@angular/core';
import { CallapiService, LatLong } from '../../callapi.service'
declare const google: any;

interface Marker {
    lat: number;
    lng: number;
    label?: string;
    draggable?: boolean;
}
@Component({
    selector: 'app-maps',
    templateUrl: './maps.component.html',
    styleUrls: ['./maps.component.css']
})
export class MapsComponent implements OnInit {
    private myLatlng: any
    constructor(private callApiService: CallapiService) { }

    ngOnInit() {

        var directionsService = new google.maps.DirectionsService();
        var directionsRenderer = new google.maps.DirectionsRenderer();
        this.myLatlng = new google.maps.LatLng(47.3200, 8.0528);
        var mapOptions = {
            zoom: 13,
            center: this.myLatlng,
            scrollwheel: false,
            styles: [{
                "featureType": "water",
                "stylers": [{
                    "saturation": 43
                }, {
                    "lightness": -11
                }, {
                    "hue": "#0088ff"
                }]
            }, {
                "featureType": "road",
                "elementType": "geometry.fill",
                "stylers": [{
                    "hue": "#ff0000"
                }, {
                    "saturation": -100
                }, {
                    "lightness": 99
                }]
            }, {
                "featureType": "road",
                "elementType": "geometry.stroke",
                "stylers": [{
                    "color": "#808080"
                }, {
                    "lightness": 54
                }]
            }, {
                "featureType": "landscape.man_made",
                "elementType": "geometry.fill",
                "stylers": [{
                    "color": "#ece2d9"
                }]
            }, {
                "featureType": "poi.park",
                "elementType": "geometry.fill",
                "stylers": [{
                    "color": "#ccdca1"
                }]
            }, {
                "featureType": "road",
                "elementType": "labels.text.fill",
                "stylers": [{
                    "color": "#767676"
                }]
            }, {
                "featureType": "road",
                "elementType": "labels.text.stroke",
                "stylers": [{
                    "color": "#ffffff"
                }]
            }, {
                "featureType": "poi",
                "stylers": [{
                    "visibility": "off"
                }]
            }, {
                "featureType": "landscape.natural",
                "elementType": "geometry.fill",
                "stylers": [{
                    "visibility": "on"
                }, {
                    "color": "#b8cb93"
                }]
            }, {
                "featureType": "poi.park",
                "stylers": [{
                    "visibility": "on"
                }]
            }, {
                "featureType": "poi.sports_complex",
                "stylers": [{
                    "visibility": "on"
                }]
            }, {
                "featureType": "poi.medical",
                "stylers": [{
                    "visibility": "on"
                }]
            }, {
                "featureType": "poi.business",
                "stylers": [{
                    "visibility": "simplified"
                }]
            }]

        };
        var map = new google.maps.Map(document.getElementById("map"), mapOptions);
        directionsRenderer.setMap(map);
        var request = {
            origin: 'Schöftland Nordweg',
            destination: 'Menziken, Maihuserstrasse',
            travelMode: 'TRANSIT',
            transitOptions: {
                departureTime: new Date(),
                modes: ['TRAIN'],
                routingPreference: 'FEWER_TRANSFERS'
            },
            unitSystem: google.maps.UnitSystem.IMPERIAL
        }
        directionsService.route(request, function (result: any, status: string) {
            if (status == 'OK') {
                directionsRenderer.setDirections(result);
            }
        });
        setInterval(() => {
            this.getlatlong();
            var marker = new google.maps.Marker({
                position: this.myLatlng,
                icon: "./assets/img/train.png",
                title: "HZMap"
            });
            marker.setMap(map);
        }, 2000)

    }

    getlatlong() {
        this.callApiService.getLatLong().subscribe((data: LatLong) => {
            this.myLatlng = new google.maps.LatLng(data.latitude, data.longitude)
        })
    }

}
