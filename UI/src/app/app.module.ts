import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SitesListComponent } from './pages/sites-list/sites-list.component';
import { Routes, RouterModule } from '@angular/router';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { AppInitService } from './services/app-init.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { MatTableModule } from '@angular/material/table'
import { MatCardModule } from '@angular/material/card';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSortModule } from '@angular/material/sort';
import { MatGridListModule } from '@angular/material/grid-list';

import { SiteDetailsComponent } from './pages/site-details/site-details.component';
import { AboutComponent } from './pages/about/about.component';
import { MatPaginatorModule } from "@angular/material/paginator";
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { NgmatTableQueryReflectorModule } from '@nghacks/ngmat-table-query-reflector';
import { SiteStatisticComponent } from './components/site-statistic/site-statistic.component';
import { StatusBadgeComponent } from './components/status-badge/status-badge.component';

export function appInit(appInitService: AppInitService) {
  return () => appInitService.Init();
}

const appRoutes: Routes = [
  { path: '', redirectTo: 'sites', pathMatch: 'full' },
  { path: 'sites', component: SitesListComponent },
  { path: 'site/:siteId', component: SiteDetailsComponent },
  { path: 'about', component: AboutComponent },
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    SitesListComponent,
    NotFoundComponent,
    SiteDetailsComponent,
    SiteStatisticComponent,    
    StatusBadgeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    RouterModule.forRoot(appRoutes),
    NgbModule,
    HttpClientModule,
    MatTableModule,
    MatCardModule,
    MatBadgeModule,
    MatChipsModule,
    MatButtonModule,
    MatTooltipModule,
    MatSortModule,
    MatGridListModule,
    MatPaginatorModule,
    NgxChartsModule,
    NgmatTableQueryReflectorModule
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInit,
      multi: true,
      deps: [AppInitService]
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: []
})
export class AppModule { }
