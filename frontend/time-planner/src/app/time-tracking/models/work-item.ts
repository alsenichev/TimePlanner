export interface WorkItem{
  id: string,
  name:string,
  durations: Duration[],
  category: string,
  nextTime: string | undefined,
  cronExpression: string | undefined,
  recurrenceStartsFrom: string | undefined,
  isAfterPreviousCompleted: boolean | undefined,
  maxRepetetionsCount: number | undefined,
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
  updateRecurrence: boolean
  cronExpression: string | undefined,
  recurrenceStartsFrom: string | undefined,
  isAfterPreviousCompleted: boolean | undefined,
  maxRepetetionsCount: number | undefined,
  sortOrder: number
}