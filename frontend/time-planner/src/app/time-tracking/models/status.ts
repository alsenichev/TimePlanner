import { WorkItem } from "./work-item";

export interface WorkingTime{
  distributed: string,
  undistributed: string,
  total: string
}
export interface Status {
  startedAt: string;
  breakStartedAt: string,
  deposit: string;
  pause: string;
  workingTime: WorkingTime,
  workItems: Array<WorkItem>
}