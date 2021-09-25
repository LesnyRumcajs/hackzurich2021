import { Component, OnInit } from '@angular/core';
import * as Chartist from 'chartist';
import {FormGroup, FormControl} from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  range = new FormGroup({
    start: new FormControl(),
    end: new FormControl()
  });
  constructor() { }
  startAnimationForLineChart(chart: Chartist.IChartistLineChart){
      let seq: any, delays: any, durations: any;
      seq = 0;
      delays = 80;
      durations = 500;

      chart.on('draw', function(data: { type: string; element: { animate: (arg0: { d?: { begin: number; dur: number; from: any; to: any; easing: Chartist.IChartistEasingDefinition; }; opacity?: { begin: number; dur: any; from: number; to: number; easing: string; }; }) => void; }; path: { clone: () => { (): any; new(): any; scale: { (arg0: number, arg1: number): { (): any; new(): any; translate: { (arg0: number, arg1: any): { (): any; new(): any; stringify: { (): any; new(): any; }; }; new(): any; }; }; new(): any; }; stringify: { (): any; new(): any; }; }; }; chartRect: { height: () => any; }; }) {
        if(data.type === 'line' || data.type === 'area') {
          data.element.animate({
            d: {
              begin: 600,
              dur: 700,
              from: data.path.clone().scale(1, 0).translate(0, data.chartRect.height()).stringify(),
              to: data.path.clone().stringify(),
              easing: Chartist.Svg.Easing.easeOutQuint
            }
          });
        } else if(data.type === 'point') {
              seq++;
              data.element.animate({
                opacity: {
                  begin: seq * delays,
                  dur: durations,
                  from: 0,
                  to: 1,
                  easing: 'ease'
                }
              });
          }
      });

      seq = 0;
  };

  ngOnInit() {

      const AnomolyChart: any = {
          labels: [325600, 3256700, 4356000, 4376000, 4354000, 4656000, 4956000],
          series: [
              [2, 2.5, 3, 2.5, 0.5, 1, 4.5]
          ]
      };

     const optionsAnomolyChart: any = {
          lineSmooth: Chartist.Interpolation.cardinal({
              tension: 0
          }),
          low: 0,
          high: 5,
          chartPadding: { top: 0, right: 0, bottom: 0, left: 0},
      }

      var anomoliesChart = new Chartist.Line('#anomolyChart', AnomolyChart, optionsAnomolyChart);

      this.startAnimationForLineChart(anomoliesChart);
  }

}
