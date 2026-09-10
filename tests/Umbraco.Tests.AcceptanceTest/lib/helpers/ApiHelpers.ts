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
    this.element = new ElementApiHelper(this);
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
  // Indexing can just take a long time sometimes, so this default leans on the longest timeout tier.
  async waitUntilItemIsIndexed(searchEndpoint: string, query: string, id: string, timeout: number = ConstantHelper.timeout.pageLoad) {
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

  /**
   * Asserts the publication state of a variant on an already-fetched document or element.
   *
   * Takes the fetched entity rather than a name on purpose: `getByName` walks the tree
   * recursively, so a name-based overload would re-request on every assertion.
   *
   * @param entityData - an entity carrying `variants`, as returned by `getByName`/`get`
   * @param expectedState - e.g. 'Published' or 'Draft'
   * @param culture - which variant to check; omit for the default (first) variant
   */
  async doesVariantHaveState(entityData: any, expectedState: string, culture: string | null = null): Promise<void> {
    const variants = entityData?.variants;
    expect(Array.isArray(variants), `Expected the entity to carry a variants array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();

    const variant = culture === null ? variants[0] : variants.find(v => v.culture === culture);
    const which = culture === null ? 'the default variant' : `culture '${culture}'`;
    expect(variant, `Expected to find ${which}, but the entity has: ${variants.map(v => v.culture).join(', ')}`).toBeTruthy();

    // Name the entity in the message - a bare toBe() failure says only "Published != Draft".
    expect(variant.state, `Expected ${which} of '${variant.name}' to be ${expectedState}`).toBe(expectedState);
  }

  /**
   * Asserts that a property on an already-fetched entity holds `expectedValue`.
   *
   * Looks the property up **by alias**, not by position: the API does not promise an order
   * for `values`, so an index-based assertion can pass or fail on seed order alone.
   *
   * @param entityData - an entity carrying `values`, as returned by `getByName`/`get`
   * @param alias - the property alias (see {@link AliasHelper.toAlias})
   * @param expectedValue - compared with `toEqual`, so objects and arrays work
   * @param culture - for a variant property; omit for invariant
   */
  async doesPropertyHaveValue(entityData: any, alias: string, expectedValue: any, culture: string | null = null): Promise<void> {
    const values = entityData?.values;
    expect(Array.isArray(values), `Expected the entity to carry a values array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();

    const matches = values.filter(v => v.alias === alias && (culture === null || v.culture === culture));
    const where = culture === null ? '' : ` for culture '${culture}'`;
    expect(matches.length, `Expected property '${alias}'${where} to be present. Present: ${values.map(v => v.alias).join(', ') || '(none)'}`).toBeGreaterThan(0);

    // A property that varies contributes one entry per culture/segment, so several matches for a
    // bare alias is legitimate rather than an error. Assert the first (what the raw `values[0]`
    // did) and name the culture in the message so a mismatch is diagnosable.
    if (culture === null && matches.length > 1) {
      const which = matches[0].culture ?? 'invariant';
      expect(matches[0].value, `Expected property '${alias}' (${which}; it varies across ${matches.length} culture/segment entries - pass a culture to target one) to equal the expected value`).toEqual(expectedValue);
      return;
    }

    expect(matches.length, `Expected exactly one property '${alias}'${where}, found ${matches.length}`).toBe(1);
    expect(matches[0].value, `Expected property '${alias}'${where} to equal the expected value`).toEqual(expectedValue);
  }

  /**
   * Asserts how many entries an already-fetched entity carries in `values`.
   *
   * Covers both content properties and data type configuration values, which share the
   * `{alias, value}` shape. Use 0 for "nothing is set".
   *
   * @param entityData - an entity carrying `values`, as returned by `getByName`/`get`
   * @param expectedCount - the number of entries expected
   */
  async doesHaveValueCount(entityData: any, expectedCount: number): Promise<void> {
    const values = entityData?.values;
    expect(Array.isArray(values), `Expected the entity to carry a values array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();
    // List what is actually set - a bare length mismatch does not say which value is unexpected.
    expect(values.length, `Expected ${expectedCount} value(s), found ${values.length}: ${values.map(v => v.alias).join(', ') || '(none)'}`).toBe(expectedCount);
  }

  /**
   * Returns the value of the entity's **only** property, asserting that there is exactly one.
   *
   * For the common single-property spec, where the alias is not otherwise in play. Like
   * {@link doesOnlyPropertyUseDataType} this states the assumption instead of burying it in
   * a `values[0]` index: if a second property appears, the assertion fails and says so,
   * rather than silently asserting against whichever property happens to sort first.
   *
   * Prefer {@link getPropertyValue} whenever the alias is known.
   */
  getOnlyPropertyValue(entityData: any): any {
    const values = entityData?.values;
    expect(Array.isArray(values), `Expected the entity to carry a values array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();

    // Assert one distinct *alias*, not one entry. A property that varies by culture or segment
    // contributes one entry per culture/segment, so a single-property document can legitimately
    // carry several entries - asserting values.length === 1 would fail on exactly the variant
    // and segment specs this is meant to serve.
    const aliases = [...new Set(values.map(v => v.alias))];
    expect(aliases.length, `Expected the entity to have exactly one property, found ${aliases.length}: ${aliases.join(', ') || '(none)'}`).toBe(1);

    // Returns the first entry, matching what the raw `values[0].value` did. When the property
    // varies, pass the culture to getPropertyValue instead of relying on this ordering.
    return values[0].value;
  }

  /** Asserts how many variants an already-fetched document or element carries. */
  async doesHaveVariantCount(entityData: any, expectedCount: number): Promise<void> {
    const variants = entityData?.variants;
    expect(Array.isArray(variants), `Expected the entity to carry a variants array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();
    expect(variants.length, `Expected ${expectedCount} variant(s), found ${variants.length}: ${variants.map(v => v.culture ?? 'invariant').join(', ')}`).toBe(expectedCount);
  }

  /**
   * Asserts how many property *definitions* a content type carries.
   *
   * Distinct from {@link doesHaveValueCount}, which counts property *values* on a content
   * item. Content types expose `properties`; content items expose `values`.
   */
  async doesHavePropertyCount(contentTypeData: any, expectedCount: number): Promise<void> {
    const properties = contentTypeData?.properties;
    expect(Array.isArray(properties), `Expected the content type to carry a properties array, got: ${JSON.stringify(contentTypeData)?.slice(0, 200)}`).toBeTruthy();
    expect(properties.length, `Expected ${expectedCount} property/ies, found ${properties.length}: ${properties.map(p => p.alias).join(', ') || '(none)'}`).toBe(expectedCount);
  }

  /** Asserts how many compositions a content type carries. */
  async doesHaveCompositionCount(contentTypeData: any, expectedCount: number): Promise<void> {
    const compositions = contentTypeData?.compositions;
    expect(Array.isArray(compositions), `Expected the content type to carry a compositions array, got: ${JSON.stringify(contentTypeData)?.slice(0, 200)}`).toBeTruthy();
    expect(compositions.length, `Expected ${expectedCount} composition(s), found ${compositions.length}`).toBe(expectedCount);
  }

  /** Asserts that the property with `propertyAlias` is backed by `dataTypeId`. */
  async doesPropertyUseDataType(contentTypeData: any, propertyAlias: string, dataTypeId: string): Promise<void> {
    const properties = contentTypeData?.properties ?? [];
    const matches = properties.filter(p => p.alias === propertyAlias);
    expect(matches.length, `Expected exactly one property '${propertyAlias}', found ${matches.length}. Present: ${properties.map(p => p.alias).join(', ')}`).toBe(1);
    expect(matches[0].dataType?.id, `Expected property '${propertyAlias}' to use data type ${dataTypeId}`).toBe(dataTypeId);
  }

  /**
   * Asserts that a content type has exactly **one** property and that it uses `dataTypeId`.
   *
   * The single-property expectation is stated rather than implied: the raw form of this
   * assertion was `properties[0].dataType.id`, where the `[0]` silently assumed both that
   * there is only one property and that it sits first.
   */
  async doesOnlyPropertyUseDataType(contentTypeData: any, dataTypeId: string): Promise<void> {
    const properties = contentTypeData?.properties ?? [];
    expect(properties.length, `Expected exactly one property, found ${properties.length}: ${properties.map(p => p.alias).join(', ') || '(none)'}`).toBe(1);
    expect(properties[0].dataType?.id, `Expected the property '${properties[0].alias}' to use data type ${dataTypeId}`).toBe(dataTypeId);
  }

  /** Asserts whether a document type is an element type. */
  async isElementType(documentTypeData: any, expected: boolean = true): Promise<void> {
    expect(documentTypeData?.isElement, `Expected '${documentTypeData?.name}' isElement to be ${expected}`).toBe(expected);
  }

  /**
   * Asserts the file content of an already-fetched template, partial view, stylesheet or script.
   *
   * @param entityData - an entity carrying `content`, as returned by `getByName`/`get`
   * @param expectedContent - the exact content expected
   */
  async doesHaveContent(entityData: any, expectedContent: string): Promise<void> {
    expect(entityData?.content, `Expected '${entityData?.name ?? entityData?.path}' to have the expected content`).toBe(expectedContent);
  }

  /**
   * Returns a property's value from an already-fetched entity, looked up **by alias**.
   *
   * Named `get*` rather than `does*` on purpose: it hands a value back, so the caller must
   * assert on it (see CLAUDE.md §3). Use it for nested shapes a generic assertion cannot
   * cover - `getPropertyValue(data, alias).markup`, `.src`, `[0].mediaKey` - which keeps the
   * nested assertion (the actual subject of such a test) while dropping the positional
   * `values[0]` lookup that assumed an order the API never promised.
   */
  getPropertyValue(entityData: any, alias: string, culture: string | null = null): any {
    const values = entityData?.values;
    expect(Array.isArray(values), `Expected the entity to carry a values array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();

    const matches = values.filter(v => v.alias === alias && (culture === null || v.culture === culture));
    const where = culture === null ? '' : ` for culture '${culture}'`;
    expect(matches.length, `Expected property '${alias}'${where} to be present. Present: ${values.map(v => v.alias).join(', ') || '(none)'}`).toBeGreaterThan(0);

    // Several matches for a bare alias means the property varies by culture or segment - one
    // entry each - which is legitimate. Return the first, as the raw `values[0].value` did;
    // pass a culture when a specific variant is the subject.
    if (culture !== null) {
      expect(matches.length, `Expected exactly one property '${alias}'${where}, found ${matches.length}`).toBe(1);
    }

    return matches[0].value;
  }

  /**
   * Asserts the name of a variant on an already-fetched document or element.
   *
   * @param entityData - an entity carrying `variants`, as returned by `getByName`/`get`
   * @param expectedName - the name expected on that variant
   * @param culture - which variant to check; omit for the default (first) variant
   */
  async doesVariantHaveName(entityData: any, expectedName: string, culture: string | null = null): Promise<void> {
    const variants = entityData?.variants;
    expect(Array.isArray(variants), `Expected the entity to carry a variants array, got: ${JSON.stringify(entityData)?.slice(0, 200)}`).toBeTruthy();

    const variant = culture === null ? variants[0] : variants.find(v => v.culture === culture);
    const which = culture === null ? 'the default variant' : `culture '${culture}'`;
    expect(variant, `Expected to find ${which}, but the entity has: ${variants.map(v => v.culture).join(', ')}`).toBeTruthy();

    expect(variant.name, `Expected ${which} to be named '${expectedName}'`).toBe(expectedName);
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
    // Round up so elapsed seconds in the current minute can't erode the promised margin.
    if (now.getSeconds() > 0 || now.getMilliseconds() > 0) {
      minute += 1;
    }
    now.setSeconds(0, 0);
    now.setMinutes(now.getMinutes() + minute);

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

  /**
   * Fixed sleep. Prefer waiting on observable state (CLAUDE.md §3); this exists for the cases
   * where there is none - notably Examine indexing, which the Management API exposes no
   * completion signal for. Leave a comment at the call site saying what it stands in for.
   * @param timeout - milliseconds; use ConstantHelper.wait.*
   */
  async waitForTimeout(timeout: number) {
    await this.page.waitForTimeout(timeout);
  }
}
