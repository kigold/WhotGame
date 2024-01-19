export interface Game {
    id: number,
    name: string,
    dateCreated: Date,
    status: string,
    playerIds: number[]    
}

export interface GameLog {
  id: number,
  message: string,
  color: string  
}