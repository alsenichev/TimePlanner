import { Component, OnInit } from '@angular/core';
import { MessageService } from '../messages/message.service';
import { Status } from './models/status';
import { WorkItem, WorkItemUpdateRequest } from './models/work-item';
import { TimeTrackingService } from './time-tracking.service';
import { FormBuilder } from '@angular/forms';
import {CdkDragDrop, moveItemInArray} from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-time-tracking',
  templateUrl: './time-tracking.component.html',
  styleUrls: ['./time-tracking.component.css']
})
export class TimeTrackingComponent implements OnInit {

  constructor(
    private timeTrackingService: TimeTrackingService,
    private messageService: MessageService,
    private fb: FormBuilder) { }
  
  active = 'today';
  newWorkItemName?:string;
  currentWorkItem?:WorkItem;
  status: Status;

  workItems? : WorkItem[];
  todayWorkItems? : WorkItem[];
  tomorrowWorkItems? : WorkItem[];
  nextWeekWorkItems? : WorkItem[];
  someDayWorkItems? : WorkItem[];
  completedWorkItems? : WorkItem[];
  scheduledWorkItems? : WorkItem[];

   filterWorkItems(items: WorkItem[]){
    this.todayWorkItems = items.filter(i => i.category == 'Today');
    this.tomorrowWorkItems = items.filter(i => i.category == 'Tomorrow');
    this.nextWeekWorkItems = items.filter(i => i.category == 'NextWeek');
    this.completedWorkItems = items.filter(i => i.category == 'Completed');
    this.completedWorkItems.sort((i,j) => Date.parse(j.completedAt!) - Date.parse(i.completedAt!));
    this.scheduledWorkItems = items.filter(i => i.category == 'Scheduled');
    this.scheduledWorkItems.sort((i,j) => Date.parse(j.nextTime!) - Date.parse(i.nextTime!));

  }

  loadWorkItems(){
    this.timeTrackingService.getWorkItems().subscribe(its => this.filterWorkItems(its));
  }
  
  loadStatus(){
    this.timeTrackingService.getStatus().subscribe(s => {
      this.status = s;
    })
  }

  createWorkItem(){
    if(this.newWorkItemName!=undefined){
      this.timeTrackingService.addWorkItem(this.newWorkItemName).subscribe(wi => {
        this.loadWorkItems();
        this.newWorkItemName = undefined;
      });
    }
  }

  onEditWorkItem(workItem:WorkItem){
    this.currentWorkItem = workItem;
  }

  onWorkItemChanged(workItem: WorkItemUpdateRequest){
    this.timeTrackingService.updateWorkItem(workItem).subscribe(wi=>{
      this.loadWorkItems();
      this.currentWorkItem = undefined;
    });
  }

  onWorkItemDeleted(workItem: WorkItem){
    this.timeTrackingService.deleteWorkItem(workItem.id).subscribe(_ =>{
      this.loadWorkItems();
      this.currentWorkItem = undefined;
      this.messageService.add(`Deleted work item: ${workItem.name}.`);
    });
  }

  dropToday(event: CdkDragDrop<string[]>) {
    this.reorderWorkItems(this.todayWorkItems!, event);
  }

  dropTomorrow(event: CdkDragDrop<string[]>) {
    this.reorderWorkItems(this.tomorrowWorkItems!, event);
  }

  dropNextWeek(event: CdkDragDrop<string[]>) {
    this.reorderWorkItems(this.nextWeekWorkItems!, event);
  }

  reorderWorkItems(source: WorkItem[], event: CdkDragDrop<string[]>){
    let workItem: WorkItem = source[event.previousIndex];
    let diff:number = event.currentIndex - event.previousIndex;
    let request: WorkItemUpdateRequest = {
      id: workItem.id,
      category: workItem.category,
      name: workItem.name,
      recurrence: workItem.recurrence,
      sortOrder: workItem.sortOrder + diff
    }
    this.timeTrackingService.updateWorkItem(request).subscribe(wi=>{
      this.loadWorkItems();
    });
  }


  ngOnInit(): void {
    this.loadWorkItems();
  }

}
