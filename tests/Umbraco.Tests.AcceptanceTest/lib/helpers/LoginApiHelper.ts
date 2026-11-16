import {ApiHelpers} from "./ApiHelpers";
import {expect, Page} from "@playwright/test";
import {ConstantHelper} from "./ConstantHelper";

/**
 * Back-office authentication against the Management API.
 *
 * Back-office auth is cookie-only: POST /security/back-office/login signs the user in and the
 * response sets the httpOnly authentication cookie. There is no authorization code, no PKCE and
 * no readable token - the cookie is the sole credential. Playwright's `page.request` shares the
 * browser context's cookie jar, so every subsequent API and UI request carries it automatically.
 */
export class LoginApiHelper {
  api: ApiHelpers;
  page: Page;

  constructor(api: ApiHelpers, page: Page) {
    this.api = api;
    this.page = page;
  }

  /**
   * Signs the given user in and leaves the authentication cookie on the browser context.
   */
  public async login(userEmail: string, password: string) {
    const response = await this.page.request.post(this.api.baseUrl + ConstantHelper.apiEndpoints.backOfficeLogin, {
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

    // Playwright shows this as the step title in the report whether or not it passes, so it reads
    // as a description rather than as a failure.
    expect(response.status(), `Sign in ${userEmail}`).toBe(ConstantHelper.statusCodes.ok);

    return response;
  }

  /**
   * Clears the authentication cookie server-side. The endpoint answers with a redirect to the
   * client logout landing, which is irrelevant here - only the cleared cookie matters.
   */
  public async signOut() {
    return await this.page.request.get(this.api.baseUrl + ConstantHelper.apiEndpoints.backOfficeSignOut, {
      headers: {
        Referer: this.api.baseUrl,
      },
      ignoreHTTPSErrors: true,
      maxRedirects: 0
    });
  }

  /**
   * Renews the session cookie. Returns false when there is no session to renew.
   */
  public async keepAlive() {
    const response = await this.page.request.post(this.api.baseUrl + ConstantHelper.apiEndpoints.backOfficeKeepAlive, {
      headers: {
        Origin: this.api.baseUrl,
      },
      ignoreHTTPSErrors: true
    });

    return response.ok();
  }

  /**
   * Probes the current session without renewing it. A non-ok response means "no session".
   */
  public async hasSession() {
    const response = await this.page.request.get(this.api.baseUrl + ConstantHelper.apiEndpoints.currentUserConfiguration, {
      ignoreHTTPSErrors: true
    });

    return response.ok();
  }
}
