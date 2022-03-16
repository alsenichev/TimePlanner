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

  loadStatus(){
    this.timeTrackingService.getStatus().subscribe(s => {
      console.log(s);
      this.status = s;
    })
  }

  ngOnInit(): void {
    this.loadStatus()
  }

}
