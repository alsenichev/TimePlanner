import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { WorkItem } from '../time-tracking/models/work-item';

@Component({
  selector: 'app-work-item-details',
  templateUrl: './work-item-details.component.html',
  styleUrls: ['./work-item-details.component.css']
})
export class WorkItemDetailsComponent {

  @Output() workItemEdit = new EventEmitter<WorkItem>();
  @Output() workItemChanged = new EventEmitter<WorkItem>();

  @Input()
  get workItem(): WorkItem { return this._workItem; }
  set workItem(workItem: WorkItem) {
    this._workItem = workItem;
    this._isComplete = workItem.completedAt != undefined
  }

  get isComplete(): boolean { return this._isComplete; }
  set isComplete(isComplete: boolean){
    this._isComplete= this.isComplete;
    if(isComplete){
      this._workItem.completedAt = new Date().toJSON();
      this._workItem.category = 'Completed';
    }else{
      this._workItem.completedAt = undefined;
      this._workItem.category = 'Today';
    }
    this.workItemChanged.emit(this._workItem);
  }

  _workItem:WorkItem;
  _isComplete:boolean;
    
  constructor() { }

  onEditWorkItem(){
    this.workItemEdit.emit(this._workItem);
  }

  today(){
    this._workItem.category = "Today";
    this.workItemChanged.emit(this._workItem)
  }
  tomorrow(){
    this._workItem.category = "Tomorrow";
    this.workItemChanged.emit(this._workItem)
  }
  nextWeek(){
    this._workItem.category = "NextWeek";
    this.workItemChanged.emit(this._workItem)
  }
}
