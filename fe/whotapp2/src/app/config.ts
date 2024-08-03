import { isDevMode } from "@angular/core";
export default {
    apiBaseUrl:
    isDevMode()
        ? 'https://localhost:7219'
        : 'https://sample.com/api/v1',
    pageSize: 10,
  };