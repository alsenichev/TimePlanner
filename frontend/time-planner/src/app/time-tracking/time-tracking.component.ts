import { Component, OnInit } from '@angular/core';
import { MessageService } from '../messages/message.service';
import { Status } from './models/status';
import { WorkItem } from './models/work-item';
import { TimeTrackingService } from './time-tracking.service';

@Component({
  selector: 'app-time-tracking',
  templateUrl: './time-tracking.component.html',
  styleUrls: ['./time-tracking.component.css']
})
export class TimeTrackingComponent implements OnInit {

  constructor(private timeTrackingService: TimeTrackingService, private messageService: MessageService) { }

  status: Status;
  selectedWorkItem? : WorkItem;

  onSelected(workItem: WorkItem){
    this.messageService.add(`StatusComponent: Selected work item: ${workItem.name}`);
    this.selectedWorkItem = workItem;
  }

  onAddNewWorkItem(){
    this.messageService.add(`Created new work item.`);
    this.selectedWorkItem = {id: "", name: "New work item.", duration:"00:00:00" };
  }

  loadStatus(){
    this.timeTrackingService.getStatus().subscribe(s => {
      console.log(s);
      this.status = s;
    })
  }
  onSubmitCurrentWorkItem(){
    if(this.selectedWorkItem?.id == "")
    {
      this.timeTrackingService.addWorkItem(this.status.id, this.selectedWorkItem).subscribe(
        {
          next: s =>{
            this.status = s;
            this.selectedWorkItem = undefined;
            this.messageService.add("Successfully created the work item.");
          },
          error: e => this.messageService.add(e.message)
        })
    }
    else{
      this.timeTrackingService.editWorkItem(this.status.id, this.selectedWorkItem!).subscribe(
        {
          next: s =>{
            this.status = s;
            this.selectedWorkItem = undefined;
            this.messageService.add("Successfully updated the work item.");
          },
          error: e => this.messageService.add(e.message)
        })
    }
  }

  ngOnInit(): void {
    this.loadStatus()
  }

}
