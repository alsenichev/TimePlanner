import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { WorkItem } from '../time-tracking/models/work-item';

@Component({
  selector: 'app-work-item-editor',
  templateUrl: './work-item-editor.component.html',
  styleUrls: ['./work-item-editor.component.css']
})
export class WorkItemEditorComponent {
  
  @Output() editComplete = new EventEmitter<WorkItem>();
  @Output() workItemDeleted = new EventEmitter<WorkItem>();

  @Input()
  get currentWorkItem(): WorkItem{ return this._currentWorkItem; }
  set currentWorkItem(workItem: WorkItem | undefined){
    if(workItem != undefined){
      this.workItemForm.patchValue({
        name: workItem.name,
        recurrence: workItem.nextTime == undefined ? undefined : 'Daily'
      });
      this._currentWorkItem = workItem;
      this.formVisible = true;
    }
  }
  _currentWorkItem: WorkItem;
  formVisible: boolean;

  constructor(private fb: FormBuilder) { }

  workItemForm = this.fb.group({
    name: ['', Validators.required],
    recurrence: ['']
  });

  cancel(){
    this.formVisible = false;
  }

  deleteWorkItem(){
    this.workItemDeleted.emit(this._currentWorkItem);
    this.formVisible = false;
  }

  onSubmit(){
    let workItem : WorkItem = {
      id: this.currentWorkItem!.id,
      name: this.workItemForm.value.name,
      recurrence: this.workItemForm.value.recurrence,
      category: this.currentWorkItem.category,
      nextTime: this.currentWorkItem.nextTime,
      durations: this.currentWorkItem.durations,
      sortOrder: this.currentWorkItem.sortOrder,
      completedAt: this.currentWorkItem.completedAt
    };
    this.formVisible = false;
    this.editComplete.emit(workItem);
  }
}
