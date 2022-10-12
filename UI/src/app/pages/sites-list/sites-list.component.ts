import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Site } from 'src/app/models/site';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import {MatPaginator} from '@angular/material/paginator';

@Component({
  selector: 'rss-sites-list',
  templateUrl: './sites-list.component.html',
  styleUrls: ['./sites-list.component.css']
})
export class SitesListComponent implements OnInit, OnDestroy {

  readonly checkStatusesIntervalInMs = 60000;
  public sitesList = new MatTableDataSource([] as Site[]);
  public isError: boolean = false;
  public error: string = null;
  public displayedColumns: string[] = ['name', 'status', 'uptime'];

  public statusFilter = 'No filter';
  public searchText = '';
  public statusFilterOptions = ['No filter', 'Only Up', 'Only Down', 'None'];
  public pageSize: number = 20;

  private interval: any;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private activatedRoute: ActivatedRoute) { }


  @ViewChild(MatPaginator) paginator: MatPaginator;
  async ngOnInit() {    
    const status = this.getStatusQuery();
    await this.updateSites();

    if (status) {
      this.sitesList.paginator.pageSize = status.pageSize;
      this.sitesList.paginator.pageIndex = status.pageIndex;
      this.pageSize = status.pageSize;
    }

    this.startTimer();

    this.sitesList.filterPredicate = function (data, filter: string): boolean {
      let fltr: Filter = JSON.parse(filter);
      let filterByText = data.name.toString().toLowerCase().includes(fltr.searchText);
      let filterByStatus = data.status.toString().toLowerCase().includes(fltr.status);
      return filterByText && filterByStatus;
    };

    if (status) {
      this.searchText = status.search;
      this.statusFilter = status.status;
      this.filterSitesList();
    }
  }

  ngOnDestroy() {
    this.pauseTimer();
  }

  @ViewChild(MatSort) sort: MatSort;

  ngAfterViewInit() {
    this.sitesList.paginator = this.paginator;
    this.sitesList.sort = this.sort;
  }

  public selectItem(row: Site) {
    this.router.navigate(['/site', row.id]);
  }

  public filterSitesList() {

    let searchText = '';
    if (!this.searchText || this.searchText.length < 2)
      searchText = '';
    else
      searchText = this.searchText.trim().toLowerCase();

    let status = '';
    switch (this.statusFilter) {
      case 'No filter': status = ''; break;
      case 'Only Up': status = 'up'; break;
      case 'Only Down': status = 'down'; break;
      case 'None': status = 'none'; break;
    }

    let filter: Filter = {
      status: status,
      searchText: searchText
    }

    this.sitesList.filter = JSON.stringify(filter);
    this.router.navigate([], { queryParams: {status: this.statusFilter, search: this.searchText }, queryParamsHandling: 'merge' });
  }

  private startTimer() {
    this.interval = setInterval(async () => {
      await this.updateSites();
    }, this.checkStatusesIntervalInMs)
  }

  private async updateSites() {
    try {
      this.sitesList.data = await this.apiService.allSites();
    }
    catch (error) {
      console.error(error);
      this.pauseTimer();
      this.error = error.message;
      this.isError = true;
    }
  }

  private pauseTimer() {
    clearInterval(this.interval);
  }

  private calculatePageSize(): number {
    const userAgent = navigator.userAgent.toLowerCase();

    const isMobile = /iPhone|Android/i.test(navigator.userAgent);
    const isTablet = /(ipad|tablet|(android(?!.*mobile))|(windows(?!.*phone)(.*touch))|kindle|playbook|silk|(puffin(?!.*(IP|AP|WP))))/.test(userAgent);

    if (isMobile) {
      return 10;
    } else if (isTablet) {
      return 15;
    } else {
      return 20;
    }
  }

  private getStatusQuery(): { status: string, search: string, pageSize: number, pageIndex: number } {
    const queryParams = this.activatedRoute.snapshot.queryParams;
    if (queryParams.hasOwnProperty('status') || queryParams.hasOwnProperty('search') || queryParams.hasOwnProperty('page_size')) {
      return {
        status: queryParams.status ?? '',
        search: queryParams.search,
        pageSize: queryParams.page_size ?? this.calculatePageSize(),
        pageIndex: queryParams.page_index
      };
    }

    return;
  }
}

class Filter {
  public searchText: string;
  public status: string;
}
