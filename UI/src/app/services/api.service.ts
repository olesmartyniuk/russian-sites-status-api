import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Site, SiteDetails } from '../models/site';
import { StatisticVm } from '../models/statistic';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl: string = 'https://dev-mss-api.herokuapp.com/';

  constructor(private http: HttpClient) { }

  public async allSites(): Promise<Site[]> {
    return this.http
      .get<Site[]>(this.baseUrl + 'api/sites')
      .toPromise();
  }

  public async siteDetails(siteId: string): Promise<SiteDetails>{
    return this.http
      .get<SiteDetails>(this.baseUrl + 'api/sites/' + siteId)
      .toPromise();
  }

  public async siteStatisticsDefault(siteId: string): Promise<StatisticVm>{
    return this.http      
      .get<StatisticVm>(this.baseUrl + 'api/sites/' + siteId + '/statistics')
      .toPromise();
  }

  public async siteStatistics(url: string): Promise<StatisticVm>{
    return this.http
      .get<StatisticVm>(this.baseUrl + url)
      .toPromise();
  }

}
