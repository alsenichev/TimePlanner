export interface WorkItem{
  id: string,
  name:string,
  durations: Duration[],
  category: string,
  nextTime: string | undefined,
  sortOrder: number,
  completedAt: string | undefined
}

export interface Duration{
  date: string,
  value: string
}