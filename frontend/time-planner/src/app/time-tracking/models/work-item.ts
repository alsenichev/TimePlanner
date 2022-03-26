export interface WorkItem{
  id: string,
  name:string,
  durations: Duration[],
  category: string,
  recurrenceDays: number | undefined,
  wakingUp: WakingUp | undefined,
  sortOrder: number,
  completedAt: string | undefined
}

export interface Duration{
  date: string,
  value: string
}

export interface WakingUp{
  when: string,
  where: string
}