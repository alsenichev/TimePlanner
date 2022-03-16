import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { Status } from './models/status';
import { MessageService } from '../messages/message.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { WorkItem } from './models/work-item';

@Injectable({
  providedIn: 'root'
})
export class TimeTrackingService {

  constructor(private http: HttpClient, private messageService: MessageService) { }

   getStatus():Observable<Status>{
    let uri = environment.apiUrl + '/statuses/current';
    return this.http.get<Status>(uri).pipe(
      catchError(e=>this.handleError(e))
    );;
  }

  addWorkItem(statusId:string, workItem: WorkItem): Observable<Status> {
    let uri = `${environment.apiUrl}/statuses/${statusId}/workItems`;
    return this.http.post<Status>(uri, {name: workItem.name})
      .pipe(
        catchError(e=>this.handleError(e))
      );
  }

  editWorkItem(statusId: string, workitem: WorkItem): Observable<Status>{
    let uri = environment.apiUrl + '/statuses/' + statusId + '/workItems/' + workitem.id + '/time/' + workitem.duration;
    return this.http.post<Status>(uri, {})
      .pipe(
        catchError(e=>this.handleError(e))
      );
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0) {
      // A client-side or network error occurred. Handle it accordingly.
      this.messageService.add('An error occurred: ' + error.error);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      let msg: string = `Backend returned code ${error.status}, body was ${error.error.ErrorMessage}`;
      this.messageService.add(msg);
    }
    // Return an observable with a user-facing error message.
    return throwError(() => new Error('Failed to handle the request.'));
  }
}
