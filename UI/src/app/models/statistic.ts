export class StatisticVm {
    navigation: Navigation;
    periods: Period[];
    data: Data;
}

export class Data {
    uptime: number;
    history: Span[];
}

export class Span {
    up: number;
    down: number;
    unknown: number;
    label: string;
}

export class Period {
    name: string;
    current: boolean;
    url: string;
}

export class Navigation {
    current: Link;
    next: Link;
    prev: Link;
}

export class Link {
    name: string;
    url: string;
}

export enum PeriodType {
    Hour,
    Day,
    Week,
    Month
}