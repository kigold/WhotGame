import { Player } from "./player"

export interface Card {
    id: number,
    name: string,
    value: number,
    isSpecial: boolean,
    color: string, //TODO change to Enum do I really need enum
    shape: string //TODO change to Enum
}

export interface CardOptionRequest{
  color: string,
  shape: string
}

export interface PlayCardRequest{
  gameId : number,
  cardId: number,
  cardColor: string,
  cardShape: string
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
  players: Player[]
}
