import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'rss-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrls: ['./status-badge.component.css']
})
export class StatusBadgeComponent implements OnInit {

  @Input() status: string;

  constructor() { }

  ngOnInit(): void {
  }

}
