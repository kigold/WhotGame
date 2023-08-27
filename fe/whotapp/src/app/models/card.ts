export interface Card {
    id: number,
    name: string,
    value: number,
    isSpecial: boolean,
    color: string, //TODO change to Enum do I really need enum
    shape: string //TODO change to Enum
}
