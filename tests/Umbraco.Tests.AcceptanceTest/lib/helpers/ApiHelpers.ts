import {Page, expect, APIResponse} from "@playwright/test"
import {umbracoConfig} from "../umbraco.config";
import {ConstantHelper} from "./ConstantHelper";
import {ReportHelper} from "./ReportHelper";
import {TelemetryDataApiHelper} from "./TelemetryDataApiHelper";
import {LanguageApiHelper} from "./LanguageApiHelper";
import {DictionaryApiHelper} from "./DictionaryApiHelper";
import {RelationTypeApiHelper} from "./RelationTypeApiHelper";
import {UserGroupApiHelper} from "./UserGroupApiHelper";
import {TemplateApiHelper} from "./TemplateApiHelper";
import {AliasHelper} from "./AliasHelper";
import {DataTypeApiHelper} from "./DataTypeApiHelper";
import {UserApiHelper} from "./UserApiHelper";
import {TemporaryFileApiHelper} from "./TemporaryFileApiHelper";
import {PackageApiHelper} from "./PackageApiHelper";
import {ScriptApiHelper} from "./ScriptApiHelper";
import {PartialViewApiHelper} from "./PartialViewApiHelper";
import {StylesheetApiHelper} from "./StylesheetApiHelper";
import * as fs from "fs";
import {LogViewerApiHelper} from "./LogViewerApiHelper";
import {DocumentTypeApiHelper} from "./DocumentTypeApiHelper";
import {DocumentApiHelper} from "./DocumentApiHelper";
import {MediaTypeApiHelper} from "./MediaTypeApiHelper";
import {MediaApiHelper} from "./MediaApiHelper";
import {ObjectTypesApiHelper} from "./ObjectTypesApiHelper";
import {ModelsBuilderApiHelper} from "./ModelsBuilderApiHelper";
import {HealthCheckApiHelper} from "./HealthCheckApiHelper";
import {PublishedCacheApiHelper} from "./PublishedCacheApiHelper";
import {RedirectManagementApiHelper} from './RedirectManagementApiHelper';
import {MemberGroupApiHelper} from './MemberGroupApiHelper';
import {MemberApiHelper} from './MemberApiHelper';
import {MemberTypeApiHelper} from "./MemberTypeApiHelper";
import {DocumentBlueprintApiHelper} from "./DocumentBlueprintApiHelper";
import {LoginApiHelper} from "./LoginApiHelper";
import {WebhookApiHelper} from "./WebhookApiHelper";
import {MediaDeliveryApiHelper} from './differentAppSettingsHelpers/MediaDeliveryApiHelper';
import {ContentDeliveryApiHelper} from "./differentAppSettingsHelpers/ContentDeliveryApiHelper";
import {SmtpApiHelper} from './SmtpApiHelper';
import {ElementApiHelper} from "./ElementApiHelper";

export class ApiHelpers {
  baseUrl: string = umbracoConfig.environment.baseUrl;
  page: Page;
  alias: AliasHelper;
  report: ReportHelper;
  telemetry: TelemetryDataApiHelper;
  language: LanguageApiHelper;
  dictionary: DictionaryApiHelper;
  relationType: RelationTypeApiHelper;
  userGroup: UserGroupApiHelper;
  template: TemplateApiHelper;
  dataType: DataTypeApiHelper;
  user: UserApiHelper;
  temporaryFile: TemporaryFileApiHelper;
  documentType: DocumentTypeApiHelper;
  document: DocumentApiHelper;
  package: PackageApiHelper;
  script: ScriptApiHelper;
  partialView: PartialViewApiHelper;
  stylesheet: StylesheetApiHelper;
  logViewer: LogViewerApiHelper;
  mediaType: MediaTypeApiHelper;
  media: MediaApiHelper;
  objectTypes: ObjectTypesApiHelper;
  modelsBuilder: ModelsBuilderApiHelper;
  healthCheck: HealthCheckApiHelper;
  publishedCache: PublishedCacheApiHelper;
  redirectManagement: RedirectManagementApiHelper;
  memberGroup: MemberGroupApiHelper;
  member: MemberApiHelper;
  memberType: MemberTypeApiHelper;
  documentBlueprint: DocumentBlueprintApiHelper;
  login: LoginApiHelper;
  webhook: WebhookApiHelper;
  mediaDeliveryApi: MediaDeliveryApiHelper;
  contentDeliveryApi: ContentDeliveryApiHelper;
  smtp: SmtpApiHelper;
  element: ElementApiHelper;

  constructor(page: Page) {
    this.page = page;
    this.alias = new AliasHelper();
    this.report = new ReportHelper(this);
    this.telemetry = new TelemetryDataApiHelper(this);
    this.language = new LanguageApiHelper(this);
    this.dictionary = new DictionaryApiHelper(this);
    this.relationType = new RelationTypeApiHelper(this);
    this.userGroup = new UserGroupApiHelper(this);
    this.template = new TemplateApiHelper(this);
    this.dataType = new DataTypeApiHelper(this);
    this.user = new UserApiHelper(this, page);
    this.temporaryFile = new TemporaryFileApiHelper(this);
    this.documentType = new DocumentTypeApiHelper(this);
    this.document = new DocumentApiHelper(this);
    this.package = new PackageApiHelper(this);
    this.script = new ScriptApiHelper(this);
    this.partialView = new PartialViewApiHelper(this);
    this.stylesheet = new StylesheetApiHelper(this);
    this.logViewer = new LogViewerApiHelper(this);
    this.mediaType = new MediaTypeApiHelper(this);
    this.media = new MediaApiHelper(this);
    this.objectTypes = new ObjectTypesApiHelper(this);
    this.modelsBuilder = new ModelsBuilderApiHelper(this);
    this.healthCheck = new HealthCheckApiHelper(this);
    this.publishedCache = new PublishedCacheApiHelper(this);
    this.redirectManagement = new RedirectManagementApiHelper(this);
    this.memberGroup = new MemberGroupApiHelper(this);
    this.member = new MemberApiHelper(this);
    this.memberType = new MemberTypeApiHelper(this);
    this.documentBlueprint = new DocumentBlueprintApiHelper(this);
    this.login = new LoginApiHelper(this, this.page);
    this.webhook = new WebhookApiHelper(this, this.page);
    this.mediaDeliveryApi = new MediaDeliveryApiHelper(this);
    this.contentDeliveryApi = new ContentDeliveryApiHelper(this);
    this.smtp = new SmtpApiHelper(this);
    this.element = new ElementApiHelper(this);
  }

  // Back-office auth is cookie-only: Playwright auto-sends the authentication cookie from the
  // browser context, so no Authorization header is set. Sending one would only make the request
  // attempt bearer-token validation, which back-office sessions have no token for.
  async getHeaders(): Promise<{ [key: string]: string; }> {
    return {};
  }

  async get(url: string, params?: { [key: string]: string | number | boolean; }, extraHeaders?: { [key: string]: string; }) {
    const headers = await this.getHeaders();
    const allHeaders = {...headers, ...extraHeaders};
    const options = {
      headers: allHeaders,
      params: params,
      ignoreHTTPSErrors: true
    }
    const response = await this.page.request.get(url, options);
    // GETs aren't asserted (they can legitimately 404/403), but a 5xx here silently yields e.g. false from
    // doesNameExist and masks the real error - surface it as a warning to aid debugging.
    if (response.status() >= 500) {
      console.warn(`GET ${url} returned server error ${response.status()}`);
    }
    return response;
  }

  async post(url: string, data?: object) {
    return await this.send('POST', url, data);
  }

  // No retry on 5xx: re-issuing a deadlocked mutation into a contended DB amplifies the contention.
  private async send(method: 'POST' | 'PUT' | 'DELETE', url: string, data?: object): Promise<APIResponse> {
    const options = {
      method: method,
      headers: await this.getHeaders(),
      data: data,
      ignoreHTTPSErrors: true
    }
    const response = await this.page.request.fetch(url, options);
    this.assertNoServerError(response);
    return response;
  }

  private assertNoServerError(response: APIResponse): void {
    expect(
      response.status(),
      `API request to ${response.url()} returned server error ${response.status()}`,
    ).toBeLessThan(500);
  }

  // Asserts a create/POST succeeded and returns the new entity id from the Location header.
  // Surfaces a failed create as a clear assertion instead of an opaque crash on a missing header.
  getIdFromLocation(response: APIResponse): string {
    expect(response.ok(), `Expected a successful response but got ${response.status()} for ${response.url()}`).toBeTruthy();
    const location = response.headers()['location'];
    expect(location, `Expected Location header to be present for ${response.url()}`).toBeTruthy();
    // Trim any trailing slash so a "/document/{id}/" Location still yields the id, not an empty segment.
    return location.replace(/\/+$/, '').split('/').pop()!;
  }

  // Examine indexes asynchronously after create; await this before a UI search so the item is findable.
  async waitUntilItemIsIndexed(searchEndpoint: string, query: string, id: string, timeout: number = ConstantHelper.timeout.veryLong) {
    await expect.poll(async () => {
      // take: 100 — the search is filtered by `query`, so the target is expected within the first page for
      // test-sized data. If a suite ever creates >100 items matching `query`, raise this or paginate.
      const response = await this.get(this.baseUrl + searchEndpoint, {query: query, take: 100});
      if (!response.ok()) {
        return false;
      }
      const body = await response.json();
      return body.items?.some((item: {id: string}) => item.id === id) ?? false;
    }, {timeout: timeout}).toBeTruthy();
  }

  async delete(url: string, data?: object) {
    return await this.send('DELETE', url, data);
  }

  async put(url: string, data?: object) {
    return await this.send('PUT', url, data);
  }

  // A non-200/blip response body lacks `items`; return empty (and warn) so a cleanup hiccup does not fail unrelated tests.
  itemsOf(json: any): any[] {
    if (!Array.isArray(json?.items)) {
      console.warn(`itemsOf: expected an items array but got: ${JSON.stringify(json)?.slice(0, 300)}`);
      return [];
    }
    return json.items;
  }

  async postMultiPartForm(url: string, id, name: string, mimeType: string, filePath) {
    const options = {
      headers: await this.getHeaders(),
      multipart: {
        Id: id,
        File: {
          name: name,
          mimeType: mimeType,
          buffer: fs.readFileSync(filePath)
        }
      },
      ignoreHTTPSErrors: true
    }
    const response = await this.page.request.post(url, options);
    this.assertNoServerError(response);
    return response;
  }

  // Ensures the shared admin session is present and belongs to the admin. Runs before every test
  // via the umbracoApi fixture, so it must stay cheap: with Security:KeepUserLoggedIn enabled the
  // cookie slides on every request and the common path is a single probe with no sign-in at all.
  async isLoginStateValid() {
    await this.refreshLoginState(umbracoConfig.user.login, umbracoConfig.user.password);
    // A live session only proves some session is valid, not that it's the admin's (a prior
    // user-switching test can leave a non-admin session). Only re-login on a positively-read
    // different user - a speculative re-login runs password verification and degrades the full run.
    const response = await this.get(this.baseUrl + ConstantHelper.apiEndpoints.currentUser);
    if (response.status() === ConstantHelper.statusCodes.ok) {
      const currentUser = await response.json();
      const currentEmail = currentUser.email?.toLowerCase();
      if (currentEmail && currentEmail !== umbracoConfig.user.login.toLowerCase()) {
        await this.signIn(umbracoConfig.user.login, umbracoConfig.user.password);
      }
    }
  }

  // Renews the session cookie, falling back to a full sign-in when there is no session to renew.
  async refreshLoginState(userEmail: string, userPassword: string) {
    if (await this.login.keepAlive()) {
      return;
    }
    await this.signIn(userEmail, userPassword);
  }

  /**
   * Signs the given user in. The server sets the httpOnly authentication cookie on the browser
   * context, which is the sole credential for every subsequent request.
   */
  async signIn(userEmail: string, userPassword: string) {
    await this.login.login(userEmail, userPassword);
  }

  /**
   * @deprecated Cookie auth issues no tokens. Use {@link signIn} instead.
   */
  async updateTokenAndCookie(userEmail: string, userPassword: string) {
    await this.signIn(userEmail, userPassword);
  }

  /**
   * Ends the current back-office session by clearing the authentication cookie server-side.
   */
  async signOut() {
    await this.login.signOut();
  }

  /**
   * @deprecated Cookie auth has no tokens to revoke. Use {@link signOut} instead.
   */
  async revokeTokens() {
    await this.signOut();
  }

  async loginToAdminUser() {
    // Signing in replaces the single authentication cookie, so there is nothing to sign out of first.
    await this.signIn(umbracoConfig.user.login, umbracoConfig.user.password);
  }

  async resetAuthState() {
    await this.signOut();
    await this.page.context().clearCookies();
  }

  async getCurrentTimePlusMinute(minute: number = 1) {
    const now = new Date();
    now.setMinutes(now.getMinutes() + minute); // Add one minute

    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  async convertDateFormat(dateString: string) {
    return new Date(dateString).toLocaleString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "numeric",
      minute: "numeric",
      second: "numeric",
      hour12: true,
    });
  }
}
