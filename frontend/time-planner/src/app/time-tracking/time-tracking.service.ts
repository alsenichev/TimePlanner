import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Status } from './models/status';
import { MessageService } from '../messages/message.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TimeTrackingService {

  constructor(private http: HttpClient, private messageService: MessageService) { }

  private statusUrl = environment.apiUrl + '/statuses/current';

  getStatus():Observable<Status>{
    return this.http.get<Status>(this.statusUrl);
  }
}
