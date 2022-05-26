import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { WorkItem, WorkItemUpdateRequest } from '../time-tracking/models/work-item';

@Component({
  selector: 'app-work-item-details',
  templateUrl: './work-item-details.component.html',
  styleUrls: ['./work-item-details.component.css']
})
export class WorkItemDetailsComponent {

  @Output() workItemEdit = new EventEmitter<WorkItem>();
  @Output() workItemChanged = new EventEmitter<WorkItemUpdateRequest>();

  @Input()
  get workItem(): WorkItem { return this._workItem; }
  set workItem(workItem: WorkItem) {
    this._workItem = workItem;
    this._isComplete = workItem.category == 'Completed'
  }

  get isComplete(): boolean { return this._isComplete; }
  set isComplete(isComplete: boolean){
    this._isComplete = this.isComplete;
    if(isComplete){
      this._workItem.category = 'Completed';
    }else{
      this._workItem.category = 'Today';
    }
    this.emitChanged();
  }

  _workItem:WorkItem;
  _isComplete:boolean;
    
  constructor() { }

  emitChanged(){
    let request : WorkItemUpdateRequest = {
      id: this._workItem.id,
      name: this._workItem.name,
      updateRecurrence: false,
      cronExpression: this._workItem.cronExpression,
      isAfterPreviousCompleted: this._workItem.isAfterPreviousCompleted,
      maxRepetetionsCount: this._workItem.maxRepetetionsCount,
      recurrenceStartsOn: this._workItem.recurrenceStartsOn,
      recurrenceEndsOn: this._workItem.recurrenceEndsOn,
      isOnPause: this._workItem.isOnPause,
      category: this._workItem.category,
      sortOrder: this._workItem.sortOrder,
    };
    this.workItemChanged.emit(request);
  }

  onEditWorkItem(){
    this.workItemEdit.emit(this._workItem);
  }

  today(){
    this._workItem.category = "Today";
    this.emitChanged();
  }
  tomorrow(){
    this._workItem.category = "Tomorrow";
    this.emitChanged();
  }
  nextWeek(){
    this._workItem.category = "NextWeek";
    this.emitChanged();
  }
}
