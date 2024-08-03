import { Card } from "./card"
import { Player } from "./player"

export interface Game {
    id: number,
    name: string,
    dateCreated: Date,
    status: string,
    playerIds: number[]    
}

export interface GameRequest {
  gameMode: string
}

export interface GameLog {
  id: number,
  message: string,
  color: string  
}

export interface GameStats {
  id: number,
  status: string,
  pick2Count: number,
  pick4Count: number,
  isTurnReversed: boolean,
  marketCount: number,
  lastPlayerId: number,
  currentPlayerId: number,
  lastPlayedCard: Card,
  players: Player[],
  aiPlayers: number[]
}

export interface PlayerGameScore {
  player: Player,
  totalCardsValue: number,
  isWinner: boolean
}

export interface GameLogResponse {
  logs: GameLog[],
  hasMore: boolean,
  skip: number
}