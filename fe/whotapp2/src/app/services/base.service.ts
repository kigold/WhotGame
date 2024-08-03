import { HttpErrorResponse } from "@angular/common/http";

export interface BaseService {
    handleError(error: HttpErrorResponse) : void
}