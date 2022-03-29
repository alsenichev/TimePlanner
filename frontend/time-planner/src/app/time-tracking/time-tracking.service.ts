import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { Status } from './models/status';
import { MessageService } from '../messages/message.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { WorkItem, WorkItemUpdateRequest } from './models/work-item';

@Injectable({
  providedIn: 'root'
})
export class TimeTrackingService {

  constructor(private http: HttpClient, private messageService: MessageService) { }

  getStatus():Observable<Status>{
    let uri = environment.apiUrl + '/statuses/current';
    return this.http.get<Status>(uri).pipe(
      catchError(e=>this.handleError(e))
    );
  }

  getWorkItems():Observable<WorkItem[]>{
    let uri = `${environment.apiUrl}/workitems`;
    return this.http.get<WorkItem[]>(uri).pipe(
      catchError(e => this.handleError(e)));
  }

  addWorkItem(workItemName: string): Observable<WorkItem> {
    let uri = `${environment.apiUrl}/workitems`;
    return this.http.post<WorkItem>(uri, {name: workItemName})
      .pipe(
        catchError(e => this.handleError(e))
      );
  }

  updateWorkItem(workItem: WorkItemUpdateRequest): Observable<WorkItem>{
    let uri = `${environment.apiUrl}/workItems/${workItem.id}`;
    return this.http.put<WorkItem>(uri, workItem)
    .pipe(
      catchError(e => this.handleError(e))
    );
  }

  deleteWorkItem(id: string): Observable<object>{
    let uri = `${environment.apiUrl}/workItems/${id}`;
    return this.http.delete(uri)
    .pipe(
      catchError(e => this.handleError(e))
    );
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0) {
      // A client-side or network error occurred. Handle it accordingly.
      this.messageService.add('An error occurred: ' + error.error);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      let msg: string = `Backend returned code ${error.status}, body was ${JSON.stringify(error)}`;
      this.messageService.add(msg);
    }
    // Return an observable with a user-facing error message.
    return throwError(() => new Error('Failed to handle the request.'));
  }
}
