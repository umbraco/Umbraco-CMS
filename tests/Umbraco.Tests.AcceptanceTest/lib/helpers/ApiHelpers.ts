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
import {IndexerApiHelper} from "./IndexerApiHelper";
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
  indexer: IndexerApiHelper;
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
    this.indexer = new IndexerApiHelper(this);
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
  }

  async getHeaders() {
    // Playwright auto-sends cookies from the browser context — no explicit Cookie header needed.
    return {
      'Authorization': 'Bearer [redacted]',
    }
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

  async isLoginStateValid() {
    await this.refreshLoginState(umbracoConfig.user.login, umbracoConfig.user.password);
    // A refresh only proves some session is valid, not that it's the admin's (a prior user-switching
    // test can leave a non-admin session). Only re-login on a positively-read different user - a
    // speculative re-login runs password verification and degrades the full run.
    const response = await this.get(this.baseUrl + ConstantHelper.apiEndpoints.currentUser);
    if (response.status() === 200) {
      const currentUser = await response.json();
      const currentEmail = currentUser.email?.toLowerCase();
      if (currentEmail && currentEmail !== umbracoConfig.user.login.toLowerCase()) {
        await this.updateTokenAndCookie(umbracoConfig.user.login, umbracoConfig.user.password);
      }
    }
  }

  async refreshLoginState(userEmail: string, userPassword: string) {
    const response = await this.page.request.post(this.baseUrl + '/umbraco/management/api/v1/security/back-office/token', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Origin: this.baseUrl
      },
      form:
        {
          grant_type: 'refresh_token',
          client_id: 'umbraco-back-office',
          redirect_uri: this.baseUrl + '/umbraco/oauth_complete',
          refresh_token: '[redacted]'
        },
      ignoreHTTPSErrors: true
    });

    if (response.status() === 200) {
      return;
    }
    console.log('Error refreshing access token, performing full re-login.');
    await this.updateTokenAndCookie(userEmail, userPassword);
    console.log('Successfully retrieved new authentication tokens.');
  }

  async updateTokenAndCookie(userEmail: string, userPassword: string) {
    await this.login.login(userEmail, userPassword);
  }

  async revokeTokens() {
    await this.page.request.post(this.baseUrl + '/umbraco/management/api/v1/security/back-office/revoke', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Origin: this.baseUrl
      },
      form: {
        token: '[redacted]',
        token_type_hint: 'access_token',
        client_id: 'umbraco-back-office'
      },
      ignoreHTTPSErrors: true
    });
    await this.page.request.post(this.baseUrl + '/umbraco/management/api/v1/security/back-office/revoke', {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Origin: this.baseUrl
      },
      form: {
        token: '[redacted]',
        token_type_hint: 'refresh_token',
        client_id: 'umbraco-back-office'
      },
      ignoreHTTPSErrors: true
    });
  }

  async loginToAdminUser() {
    await this.revokeTokens();
    await this.updateTokenAndCookie(umbracoConfig.user.login, umbracoConfig.user.password);
  }

  async resetAuthState() {
    await this.revokeTokens();
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
