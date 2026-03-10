import {ApiHelpers} from "./ApiHelpers";
import {Page} from "@playwright/test";
import {createHash} from "crypto";

export class LoginApiHelper {
  api: ApiHelpers;
  page: Page;

  constructor(api: ApiHelpers, page: Page) {
    this.api = api;
    this.page = page;
  }

  public async login(userEmail: string, password: string) {
    const codeVerifier = "12345";
    const stateValue = 'myStateValue';
    const sessionCookie = await this.authenticate(userEmail, password);
    const codeChallenge = this.createCodeChallenge(codeVerifier);
    const authorizationResponse = await this.authorize(codeChallenge, sessionCookie, stateValue);
    const PKCECookie = this.extractPKCECookie(authorizationResponse);
    await this.exchangeCodeForTokens(sessionCookie, codeVerifier, PKCECookie);
  }

  private extractPKCECookie(setCookies: string) {
    const match = setCookies.match(/.*(__Host-umbPkceCode=[A-Za-z0-9_-]+;)/s);
    return match?.[1] ?? "";
  }

  private async authenticate(userEmail: string, password: string){
    const response = await this.page.request.post(this.api.baseUrl + '/umbraco/management/api/v1/security/back-office/login', {
      headers: {
        'Content-Type': 'application/json',
        Referer: this.api.baseUrl,
        Origin: this.api.baseUrl,
      },
      data: {
        username: userEmail,
        password: password
      },
      ignoreHTTPSErrors: true
    });

    return response.headers()['set-cookie'];
  }

  private createCodeChallenge(codeVerifier: string) {
    return createHash('sha256').update(codeVerifier, 'utf8').digest('base64').replace(/=/g, '').trim();
  }

  private async authorize(codeChallenge: string, cookie: string, stateValue: string){
    const authorizationUrl = `${this.api.baseUrl}/umbraco/management/api/v1/security/back-office/authorize?client_id=umbraco-back-office&response_type=code&redirect_uri=${encodeURIComponent(this.api.baseUrl + '/umbraco/oauth_complete')}&code_challenge_method=S256&code_challenge=${codeChallenge}&state=${stateValue}&scope=offline_access&prompt=consent&access_type=offline`;
    const response = await this.page.request.get(authorizationUrl, {
      headers: {
        Cookie: cookie,
        Referer: this.api.baseUrl,
      },
      ignoreHTTPSErrors: true,
      maxRedirects: 0
    });

    if (response.status() !== 302) {
      console.error('Authorization failed');
    }
    return response.headers()['set-cookie'];
  }

  private async exchangeCodeForTokens(cookie: string, codeVerifier: string, pkceCookie: string) {
    const response = await this.page.request.post(this.api.baseUrl + '/umbraco/management/api/v1/security/back-office/token', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Cookie: pkceCookie + cookie,
        Origin: this.api.baseUrl
      },
      form: {
        grant_type: 'authorization_code',
        client_id: 'umbraco-back-office',
        redirect_uri: this.api.baseUrl + '/umbraco/oauth_complete',
        code: '[redacted]',
        code_verifier: codeVerifier
      },
      ignoreHTTPSErrors: true
    });

    if (response.status() !== 200) {
      console.error('Token exchange failed');
    }
  }
}
