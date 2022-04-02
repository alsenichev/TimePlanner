export interface WorkItem{
  id: string,
  name:string,
  durations: Duration[],
  category: string,
  nextTime: string | undefined,
  cronExpression: string | undefined,
  recurrenceStartsOn: string | undefined,
  recurrenceEndsOn: string | undefined,
  isAfterPreviousCompleted: boolean | undefined,
  maxRepetetionsCount: number | undefined,
  isOnPause: boolean | undefined,
  sortOrder: number,
  completedAt: string | undefined
}

export interface Duration{
  date: string,
  value: string
}

export interface WorkItemUpdateRequest{
  id: string,
  name: string,
  category: string,
  updateRecurrence: boolean,
  cronExpression: string | undefined,
  recurrenceStartsOn: string | undefined,
  recurrenceEndsOn: string | undefined,
  isAfterPreviousCompleted: boolean | undefined,
  maxRepetetionsCount: number | undefined,
  isOnPause: boolean | undefined,
  sortOrder: number
}