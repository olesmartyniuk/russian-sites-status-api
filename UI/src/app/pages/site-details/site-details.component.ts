import { Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Server, SiteDetails } from 'src/app/models/site';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common'
import * as moment from 'moment';

@Component({
  selector: 'rss-site-details',
  templateUrl: './site-details.component.html',
  styleUrls: ['./site-details.component.css']
})
export class SiteDetailsComponent implements OnInit, OnDestroy {

  readonly checkStatusesIntervalInMs = 60000;

  public site: SiteDetails;
  public isError: boolean = false;
  public error: string = null;
  public displayedColumns: string[] = ['region', 'status', 'time'];

  private interval: any;
  private siteId: string;

  constructor(
    private route: ActivatedRoute,
    private location: Location,
    private apiService: ApiService
  ) { }

  async ngOnInit() {
    this.siteId = this.route.snapshot.paramMap.get('siteId');

    await this.updateSiteInfo();

    this.startTimer();
  }

  ngOnDestroy() {
    this.pauseTimer();
  }

  public back = () => {
    this.location.back();
  }

  public getReadableTime = (lastDate) => {
    var minDate = moment.utc('0001-01-01');

    if (moment.utc(lastDate).isAfter(minDate)) {
      return moment(lastDate).fromNow();
    }

    return '-';
  }

  public getTime(server: Server): string {
    if (server.status === 'up') {
      if (server.spentTimeInSec === 0) {
        return '< 1 sec';
      }
      return `~ ${server.spentTimeInSec} sec`;
    }

    switch (server.statusCode) {
      case 0:
        return `Timeout`;
      case 403:
        return `Forbidden ${server.statusCode}`;
      case 404:
        return `Not found ${server.statusCode}`;
      case 500:
        return `Site error ${server.statusCode}`;
      case 504:
        return `Gateway timeout ${server.statusCode}`;
      case 301:
        return `Redirect ${server.statusCode}`;
      case 520:
        return `Unknown ${server.statusCode}`;
      default:
        return `HTTP ${server.statusCode}`;
    }
  }

  private startTimer() {
    this.interval = setInterval(async () => {
      await this.updateSiteInfo();
    }, this.checkStatusesIntervalInMs)
  }

  private pauseTimer() {
    clearInterval(this.interval);
  }

  private async updateSiteInfo() {
    try {
      this.site = await this.apiService.siteDetails(this.siteId);
    }
    catch (error) {
      console.error(error);
      this.pauseTimer();
      this.error = error.message;
      this.isError = true;
    }
  }
}