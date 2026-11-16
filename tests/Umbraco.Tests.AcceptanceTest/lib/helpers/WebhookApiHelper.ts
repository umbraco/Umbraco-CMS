import {expect, Page} from "@playwright/test";
import {ApiHelpers} from "./ApiHelpers";
import {WebhookBuilder} from "../builders";
import {ConstantHelper} from "./ConstantHelper";
import {randomUUID} from "crypto";
import {WebhookReceiver} from "./WebhookReceiver";

export class WebhookApiHelper {
  api: ApiHelpers;
  page: Page;
  /**
   * The receiver's base URL, known once a token has been generated or a webhook has been created.
   */
  receiverUrl: string = '';

  constructor(api: ApiHelpers, page: Page) {
    this.api = api;
    this.page = page;
  }

  /**
   * @deprecated Use {@link receiverUrl} instead.
   */
  get webhookSiteUrl() {
    return this.receiverUrl;
  }

  set webhookSiteUrl(url: string) {
    this.receiverUrl = url;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/webhook/' + id);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/webhook/' + id);
    return response.status() === 200;
  }

  async create(webhookData) {
    if (webhookData == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/webhook', webhookData);
    // Returns the id of the created webhook
    return this.api.getIdFromLocation(response);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/webhook/' + id);
  }

  async update(webhook: object) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/webhook/', webhook);
  }

  async getAll() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/webhook?pageSize=50&skip=0&take=10000');
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getByName(name: string) {
    const allWebhooks = await this.getAll();
    const jsonWebhooks = await allWebhooks.json();

    for (const webhook of this.api.itemsOf(jsonWebhooks)) {
      if (webhook.name === name) {
        return await this.get(webhook.id);
      }
    }
    return null;
  }

  async ensureNameNotExists(name: string) {
    const allWebhooks = await this.getAll();
    const jsonWebhooks = await allWebhooks.json();

    for (const webhook of this.api.itemsOf(jsonWebhooks)) {
      if (webhook.name === name) {
        return await this.delete(webhook.id);
      }
    }
    return null;
  }

  async generateReceiverToken() {
    await this.startReceiver();
    return randomUUID();
  }

  private async startReceiver() {
    // A URL that has been set on the helper is kept, so webhooks can be pointed at another receiver.
    this.receiverUrl ||= await WebhookReceiver.start();
    return this.receiverUrl;
  }

  /**
   * @deprecated Use {@link generateReceiverToken} instead. The token is no longer a webhook.site token: requests
   * are received in the test process, so the site under test must run on the same machine.
   */
  async generateWebhookSiteToken() {
    return await this.generateReceiverToken();
  }

  /**
   * Waits for requests to be received for the token.
   *
   * @param expectedContentFragment When given, only requests whose content contains it are waited for and returned.
   * @returns The received requests, or null when none were received within the timeout.
   */
  async getReceivedRequests(receiverToken: string, expectedContentFragment?: string, timeout: number = ConstantHelper.timeout.navigation, pollInterval: number = ConstantHelper.timeout.short) {
    const deadline = Date.now() + timeout;

    while (true) {
      const requests = WebhookReceiver.getRequests(receiverToken);
      const matches = expectedContentFragment === undefined
        ? requests
        : requests.filter((request) => request.content.includes(expectedContentFragment));
      if (matches.length > 0) {
        return matches;
      }
      if (Date.now() >= deadline) {
        return null;
      }

      await new Promise((resolve) => setTimeout(resolve, pollInterval));
    }
  }

  /**
   * @deprecated Use {@link getReceivedRequests} instead. The returned requests only hold the method, url,
   * content and headers.
   */
  async getWebhookSiteRequestResponse(webhookSiteToken: string, timeoutMs: number = ConstantHelper.timeout.veryLong, pollInterval: number = ConstantHelper.timeout.short, expectedContentFragment?: string) {
    return await this.getReceivedRequests(webhookSiteToken, expectedContentFragment, timeoutMs, pollInterval);
  }

  async doesWebhookHaveEvent(webhookName: string, eventName: string) {
    const webhookData = await this.getByName(webhookName);
    return webhookData.events.find(event => event.eventName === eventName);
  }

  async doesWebhookHaveContentTypeId(webhookName: string, contentTypeId: string) {
    const webhookData = await this.getByName(webhookName);
    return webhookData.contentTypeKeys.includes(contentTypeId);
  }

  async doesWebhookHaveHeader(webhookName: string, headerName: string, headerValue: string) {
    const webhookData = await this.getByName(webhookName);
    return webhookData.headers[headerName] === headerValue;
  }

  async doesWebhookHaveUrl(webhookName: string, url: string) {
    const webhookData = await this.getByName(webhookName);
    return webhookData.url === url;
  }

  async isWebhookEnabled(webhookName: string, isEnabled: boolean = true) {
    const webhookData = await this.getByName(webhookName);
    return expect(webhookData.enabled).toBe(isEnabled);
  }

  async getEventTypeValue(eventName: string) {
    const eventData = ConstantHelper.webhookEvents.find(event => event.eventName === eventName);
    return eventData?.eventType || '';
  }

  async getEventAliasValue(eventName: string) {
    const eventData = ConstantHelper.webhookEvents.find(event => event.eventName === eventName);
    return eventData?.alias || '';
  }

  async createDefaultWebhook(webhookName: string, receiverToken: string, eventName: string = 'Content Published', isEnabled: boolean = true) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = await this.startReceiver() + receiverToken;
    const eventAlias = await this.getEventAliasValue(eventName);

    const webhook = new WebhookBuilder()
      .withName(webhookName)
      .withUrl(webhookUrl)
      .withEventAlias(eventAlias)
      .withEnabled(isEnabled)
      .build();

    return await this.create(webhook);
  }

  async createWebhookForSpecificContentType(webhookName: string, receiverToken: string, eventName: string, contentTypeName: string) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = await this.startReceiver() + receiverToken;
    const eventAlias = await this.getEventAliasValue(eventName);
    const eventType = await this.getEventTypeValue(eventName);
    let contentTypeData;
    if (eventType == 'Content') {
      contentTypeData = await this.api.documentType.getByName(contentTypeName);
    } else {
      contentTypeData = await this.api.mediaType.getByName(contentTypeName);
    }

    const webhook = new WebhookBuilder()
      .withName(webhookName)
      .withUrl(webhookUrl)
      .withEventAlias(eventAlias)
      .withContentTypeKey(contentTypeData.id)
      .build();

    return await this.create(webhook);
  }

  async createWebhookWithHeader(webhookName: string, receiverToken: string, eventName: string, headerName: string, headerValue: string) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = await this.startReceiver() + receiverToken;
    const eventAlias = await this.getEventAliasValue(eventName);

    const webhook = new WebhookBuilder()
      .withName(webhookName)
      .withUrl(webhookUrl)
      .withEventAlias(eventAlias)
      .withHeader(headerName, headerValue)
      .build();

    return await this.create(webhook);
  }
}
