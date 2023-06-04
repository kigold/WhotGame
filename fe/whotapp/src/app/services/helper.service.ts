import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import {MatSnackBarModule, MAT_SNACK_BAR_DATA, MatSnackBar} from '@angular/material/snack-bar';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HelperService {

  constructor(private snackBar: MatSnackBar) { }

  toast(message: string){    
		this.snackBar.open(message);
  }

  handleError(error: HttpErrorResponse){
		this.toast(error.message);

		if (error.status === 0) {
			// A client-side or network error occurred. Handle it accordingly.
			console.error('An error occurred:', error.error);
		  } else {
			// The backend returned an unsuccessful response code.
			// The response body may contain clues as to what went wrong.
			console.error(
			  `Backend returned code ${error.status}, body was: `, error.error);
		  }
		  // Return an observable with a user-facing error message.
		  return throwError(() => new Error('Something bad happened; please try again later.'));
	}
}
