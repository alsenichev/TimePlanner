import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { WorkItem, WorkItemUpdateRequest } from '../time-tracking/models/work-item';

@Component({
  selector: 'app-work-item-editor',
  templateUrl: './work-item-editor.component.html',
  styleUrls: ['./work-item-editor.component.css']
})
export class WorkItemEditorComponent {
  
  @Output() editComplete = new EventEmitter<WorkItemUpdateRequest>();
  @Output() workItemDeleted = new EventEmitter<WorkItem>();

  @Input()
  get currentWorkItem(): WorkItem{ return this._currentWorkItem; }
  set currentWorkItem(workItem: WorkItem | undefined){
    if(workItem != undefined){
      this.workItemForm.patchValue({
        name: workItem.name,
        cronExpression: workItem.cronExpression,
        isAfterPreviousCompleted: workItem.isAfterPreviousCompleted
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
    cronExpression: [''],
    isAfterPreviousCompleted: ['']
  });

  cancel(){
    this.formVisible = false;
  }

  deleteWorkItem(){
    this.workItemDeleted.emit(this._currentWorkItem);
    this.formVisible = false;
  }

  onSubmit(){
    let workItemRequest : WorkItemUpdateRequest = {
      id: this.currentWorkItem.id,
      name: this.workItemForm.value.name,
      updateRecurrence: this.workItemForm.value.cronExpression != undefined,
      cronExpression: this.workItemForm.value.cronExpression,
      isAfterPreviousCompleted: this.workItemForm.value.isAfterPreviousCompleted,
      maxRepetetionsCount: undefined,
      recurrenceStartsOn: undefined,
      recurrenceEndsOn: undefined,
      category: this.currentWorkItem.category,
      isOnPause: undefined,
      sortOrder: this.currentWorkItem.sortOrder,
    };
    this.formVisible = false;
    this.editComplete.emit(workItemRequest);
  }
}
