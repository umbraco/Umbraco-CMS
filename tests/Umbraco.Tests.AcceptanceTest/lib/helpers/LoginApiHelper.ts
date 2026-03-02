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
    const codeVerifier = "12345"; // A static state value for testing
    const stateValue = 'myStateValue'; // A static state value for testing
    const cookie = await this.getCookie(userEmail, password);
    const codeChallenge = await this.createCodeChallenge(codeVerifier);
    const authorizationSetCookie = await this.getAuthorizationSetCookie(codeChallenge, cookie, stateValue);
    const PKCECookie = await this.extractPKCECodeFromSetCookie(authorizationSetCookie);
    const setCookies = await this.getCookiesWithAccessTokenAndRefreshToken(cookie, codeVerifier, PKCECookie);
    return {cookie, setCookies};
  }

  async extractPKCECodeFromSetCookie(setCookies: string) {
    const match = setCookies.match(/.*(__Host-umbPkceCode=[A-Za-z0-9_-]+;)/s);
    return match?.[1] ?? "";
  }

  async getCookie(userEmail: string, password: string) {
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

    // Ensure the cookie is properly captured
    return response.headers()['set-cookie'];
  }

  async createCodeChallenge(codeVerifier: string) {
    return createHash('sha256').update(codeVerifier, 'utf8').digest('base64').replace(/=/g, '').trim();
  }

  async getAuthorizationSetCookie(codeChallenge: string, cookie: string, stateValue: string) {
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
      console.error('Failed to find cookie');
    }
    return response.headers()['set-cookie'];
  }

  async getCookiesWithAccessTokenAndRefreshToken(cookie: string, codeVerifier: string, PKCECookie: string) {
    const response = await this.page.request.post(this.api.baseUrl + '/umbraco/management/api/v1/security/back-office/token', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Cookie: PKCECookie + cookie,
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
      console.error('Failed to retrieve cookie');
    }
    return response.headers()['set-cookie'];
  }

  async getAccessToken(cookie: string, refreshToken: string) {
    const response = await this.page.request.post(this.api.baseUrl + '/umbraco/management/api/v1/security/back-office/token', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Cookie: cookie,
        Origin: this.api.baseUrl
      },
      form: {
        grant_type: 'refresh_token',
        client_id: 'umbraco-back-office',
        redirect_uri: this.api.baseUrl + '/umbraco/oauth_complete',
        refresh_token: refreshToken,
      },
      ignoreHTTPSErrors: true
    });

    if (response.status() === 200) {
      console.log('Login successful');
    } else {
      console.error('Login failed');
    }
    return await response.json();
  }
}
