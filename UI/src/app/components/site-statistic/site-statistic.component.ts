import { Component, Input, OnInit } from '@angular/core';
import { StatisticVm as Statistic } from 'src/app/models/statistic';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-site-statistic',
  templateUrl: './site-statistic.component.html',
  styleUrls: ['./site-statistic.component.css']
})
export class SiteStatisticComponent implements OnInit {

  @Input() siteId: string = null; 

  chartData: any[];  
  colorScheme = {
    domain: ['red', 'green', 'grey']
  };

  statistics: Statistic;

  constructor(private apiService: ApiService) { }

  async ngOnInit() {
    await this.updateStatistics(null);
  }

  public async periodClick(url: string) {
    await this.updateStatistics(url);
  }

  private async updateStatistics(url: string) {
    if (url === null) {
      this.statistics = await this.apiService.siteStatisticsDefault(this.siteId);
    } else {
      this.statistics = await this.apiService.siteStatistics(url);
    }

    this.chartData = this.statistics.data.history
      .map(item => ({
        name: item.label,
        series: [
          {
            name: 'down',
            value: item.down
          },
          {
            name: 'up',
            value: item.up
          },
          {
            name: 'unknown',
            value: item.unknown
          }
        ]
      }));
  }
}
