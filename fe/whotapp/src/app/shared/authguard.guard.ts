import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';

export const authguardGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService)
  if (!authService.getUserProfile())
    router.navigate(['home']);
  return true;
};
