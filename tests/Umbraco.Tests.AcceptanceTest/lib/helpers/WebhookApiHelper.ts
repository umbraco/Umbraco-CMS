import {expect, Page} from "@playwright/test";
import {ApiHelpers} from "./ApiHelpers";
import {WebhookBuilder} from "../builders";
import {ConstantHelper} from "./ConstantHelper";

export class WebhookApiHelper {
  api: ApiHelpers;
  page: Page;
  webhookSiteUrl: string = 'https://webhook.site/';

  constructor(api: ApiHelpers, page: Page) {
    this.api = api;
    this.page = page;
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
    return response.headers().location.split("/").pop();
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

    for (const webhook of jsonWebhooks.items) {
      if (webhook.name === name) {
        return await this.get(webhook.id);
      }
    }
    return null;
  }

  async ensureNameNotExists(name: string) {
    const allWebhooks = await this.getAll();
    const jsonWebhooks = await allWebhooks.json();

    for (const webhook of jsonWebhooks.items) {
      if (webhook.name === name) {
        return await this.delete(webhook.id);
      }
    }
    return null;
  }

  async generateWebhookSiteToken() {
    const createWebhookResponse = await this.page.request.post(this.webhookSiteUrl + 'token', {
      headers: {
        'Accept': 'application/json'
      }
    });
    const webhookData = await createWebhookResponse.json();
    const webhookToken = webhookData.uuid;
    return webhookToken;
  }

  async getWebhookSiteRequestResponse(webhookSiteToken: string, timeoutMs: number = 15000, pollInterval: number = 1000) {
    const requestUrl = this.webhookSiteUrl + 'token/' + webhookSiteToken + '/requests';
    const start = Date.now();

    while (Date.now() - start < timeoutMs) {
      const requestResponse = await this.page.request.get(requestUrl, {
        headers: {
          'Accept': 'application/json'
        }
      });
      const requestJson = await requestResponse.json();
      if (requestJson.total > 0) {
        return requestJson.data;
      }
      
      // Polling again if there is no webhook received yet
      await new Promise((resolve) => setTimeout(resolve, pollInterval));
    }
    return null;
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

  async createDefaultWebhook(webhookName: string, webhookSiteToken: string, eventName: string = 'Content Published', isEnabled: boolean = true) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = this.webhookSiteUrl + webhookSiteToken;
    const eventAlias = await this.getEventAliasValue(eventName);

    const webhook = new WebhookBuilder()
      .withName(webhookName)
      .withUrl(webhookUrl)
      .withEventAlias(eventAlias)
      .withEnabled(isEnabled)
      .build();

    return await this.create(webhook);
  }

  async createWebhookForSpecificContentType(webhookName: string, webhookSiteToken: string, eventName: string, contentTypeName: string) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = this.webhookSiteUrl + webhookSiteToken;
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

  async createWebhookWithHeader(webhookName: string, webhookSiteToken: string, eventName: string, headerName: string, headerValue: string) {
    await this.ensureNameNotExists(webhookName);
    const webhookUrl = this.webhookSiteUrl + webhookSiteToken;
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