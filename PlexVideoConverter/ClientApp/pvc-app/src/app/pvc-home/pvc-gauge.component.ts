import { Component, computed, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { PvcAppStore } from '../store/pvc-app.signal.store';
import {
  ApexChart,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ApexResponsive,
  ChartComponent,
  NgApexchartsModule
} from "ng-apexcharts";

export type ChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: string[];
  colors: string[];
  legend: ApexLegend;
  plotOptions: ApexPlotOptions;
  responsive: ApexResponsive[];
};

@Component({
  selector: 'pvc-gauge',
  imports: [
    CommonModule,
    MatCardModule,
    NgApexchartsModule,
  ],
  styleUrl: './pvc-home.component.css',
  template: `
    <mat-card class="example-card" appearance="outlined" style="max-width: 450px">
    <mat-card-header>
      <mat-card-title>Free Space Card</mat-card-title>
      <mat-card-subtitle>Space could be saved</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <div id="chart">
        <apx-chart
          [series]="chartOptions.series"
          [chart]="chartOptions.chart"
          [plotOptions]="chartOptions.plotOptions"
          [labels]="chartOptions.labels"
          [legend]="chartOptions.legend"
          [colors]="chartOptions.colors"
          [responsive]="chartOptions.responsive"
        ></apx-chart>
      </div>
    </mat-card-content>
    </mat-card>
  `
})
export class PvcGaugeComponent implements OnInit {
  @ViewChild("chart") chart: ChartComponent = new ChartComponent();
  public chartOptions: ChartOptions;

  pvcAppStore = inject(PvcAppStore);

  _wdStats = computed(() => {
    return this.pvcAppStore.wdStats()
  });

  folderName = '';
  constructor() {

    this.chartOptions = {
      series: [100, 67],
      chart: {
        height: 390,
        type: "radialBar"
      },
      plotOptions: {
        radialBar: {
          offsetY: 0,
          startAngle: 45,
          endAngle: 315,
          hollow: {
            margin: 5,
            size: "30%",
            background: "transparent",
            image: undefined
          },
          dataLabels: {
            name: {
              show: false
            },
            value: {
              show: false
            }
          }
        }
      },
      colors: ["#FF5733", "#228B22"],
      labels: ["Current Size", "After Conversion"],
      legend: {
        show: true,
        floating: true,
        fontSize: "16px",
        position: "left",
        offsetX: 50,
        offsetY: 10,
        labels: {
          useSeriesColors: true
        },
        formatter: function(seriesName, opts) {
          return seriesName + ":  " + opts.w.globals.series[opts.seriesIndex];
        },
        itemMargin: {
          horizontal: 3
        }
      },
      responsive: [
        {
          breakpoint: 480,
          options: {
            legend: {
              show: false
            }
          }
        }
      ]
    };

  }

  ngOnInit() {
    const stats = this.pvcAppStore.wdStats;

    console.log(JSON.stringify(stats));
  }

}
