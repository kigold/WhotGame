export interface ResponseModel<T> {
    totalCount: number,
    code: string,
    errors: string[],
    description: string,
    payload: T,
    hasError: boolean
}
