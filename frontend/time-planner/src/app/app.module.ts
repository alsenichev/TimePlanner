import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TimeTrackingComponent } from './time-tracking/time-tracking.component';
import { MessagesComponent } from './messages/messages.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { WorkItemEditorComponent } from './work-item-editor/work-item-editor.component';
import { WorkItemDetailsComponent } from './work-item-details/work-item-details.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { CustomDateParserFormatter } from './work-item-editor/date-adapters';

@NgModule({
  declarations: [
    AppComponent,
    TimeTrackingComponent,
    MessagesComponent,
    WorkItemEditorComponent,
    WorkItemDetailsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    NgbModule,
    DragDropModule
  ],
  providers: [
    {provide: NgbDateParserFormatter, useClass: CustomDateParserFormatter}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
