import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AppInitService {

  public async Init(): Promise<void> {
    // do something on App start
  }
}
