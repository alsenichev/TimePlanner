import { Injectable } from '@angular/core';
import { Alert } from './alert';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor() { }

  alerts: Alert[] = [];

  addInfo(message: string) {
    this.alerts.push({type: 'info', message: message});
  }

  addError(message: string){
    this.alerts.push({type: 'danger', message: message});
  }

  close(alert: Alert) {
    this.alerts.splice(this.alerts.indexOf(alert), 1);
  }
}
