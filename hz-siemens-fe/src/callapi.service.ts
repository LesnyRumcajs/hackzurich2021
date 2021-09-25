import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Double } from 'bson';

export interface LatLong{
  latitude: Double,
  longitude: Double
}

@Injectable({
  providedIn: 'root'
})
export class CallapiService {

  
  constructor(private http: HttpClient) { }

  public getLatLong() {
    return this.http.get<LatLong>('/api/getLatLong');
  }

}
