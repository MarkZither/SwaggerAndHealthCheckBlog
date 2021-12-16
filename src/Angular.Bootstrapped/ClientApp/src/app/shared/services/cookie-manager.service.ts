import { Injectable } from '@angular/core';
import { AbstractSecurityStorage } from 'angular-auth-oidc-client';
import { CookieService } from 'ngx-cookie-service';
@Injectable({
  providedIn: 'root'
})
export class CookieManagerService implements AbstractSecurityStorage {
  constructor(private cookieService: CookieService) { }
    remove(key: string): void {
        throw new Error('Method not implemented.');
    }
    clear(): void {
        throw new Error('Method not implemented.');
    }
  read(key: string) {
    let item = this.cookieService.get(key);
    if (!!item) {
      return JSON.parse(item);
    }
    else {
      return null;
    }
  }
  write(key: string, value: any) {
    value = value || null;
    //Expiration time can be set in the third parameter of below function.
    this.cookieService.set(`${key}`, JSON.stringify(value), undefined, undefined, undefined, true, "Strict");
    return true;
  }
}
