import {expect} from "@playwright/test";
import {ApiHelpers} from "../ApiHelpers";

export class ContentDeliveryApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAllContentItems() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content?skip=0&take=10000');
    return await response.json();
  }

  async getContentItemWithId(id: string, extraHeaders?: { [key: string]: string; }, expand?: string, fields?: string) {
    if (expand || fields) {
      let query = '?';  
      if (expand) {
        query += 'expand=' + expand;
      }
      if (fields) {
        query += (query.length > 1 ? '&' : '') + 'fields=' + fields;
      }
      return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content/item/' + id + query, undefined, extraHeaders);
    } else {
      return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content/item/' + id, undefined, extraHeaders);
    }
  }

  async getContentItemWithRoute(route: string, extraHeaders?: { [key: string]: string; }) {
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content/item' + route, undefined, extraHeaders);
  }

  async getContentItemsWithIds(ids: string[], extraHeaders?: { [key: string]: string; }) {
    let query = '?';
    for (let i = 0; i < ids.length; i++) {
      query += 'id=' + ids[i] + (i < ids.length - 1 ? '&' : ''); 
    }
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content/items' + query, undefined, extraHeaders);
  }

  async getContentItemsFromAQuery(extraHeaders?: { [key: string]: string; }, fetch?: string, filter?: string, sort?: string, skip?: number, take?: number) {
    let query = '';
    if (fetch) {
      query += ' fetch=' + fetch;
    }
    if (filter) {
      query += ' filter=' + filter;
    }
    if (sort) {
      query += ' sort=' + sort;
    }
    if (skip !== undefined) {
      query += ' skip=' + skip;
    }
    if (take !== undefined) {
      query += ' take=' + take;
    }
    if (query !== '') {
      query = '?' + query.trim().replace(/ /g, '&');
    }

    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/content' + query, undefined, extraHeaders);
  }

  async verifyBasicPropertiesForContentItem(contentName: string, contentItemJson) {
    const contentData = await this.api.document.getByName(contentName);

    // Verify name, createDate, updateDate, id and contentType are always included in the response
    expect(contentItemJson.name).toBe(contentName);
    expect(new Date(contentItemJson.createDate)).toEqual(new Date(contentData.variants[0].createDate));
    expect(new Date(contentItemJson.createDate)).toEqual(new Date(contentData.variants[0].createDate));
    expect(contentItemJson.id).toBe(contentData.id);
    const contentTypeData = await this.api.documentType.get(contentData.documentType.id);
    expect(contentItemJson.contentType).toBe(contentTypeData.alias);
  }

  async verifyRoutePropertyForContentItem(contentName: string, contentItemJson, expectedRoutePath?: string) {
    // Verify route property
    if (expectedRoutePath !== undefined) {
      expect(contentItemJson.route.path).toBe(expectedRoutePath);
    } else {
      const contentData = await this.api.document.getByName(contentName);
      const contentUrl = await this.api.document.getDocumentUrl(contentData.id);
      expect(contentItemJson.route.path).toBe(contentUrl);
    }
  }

  async verifyEditorialPropertiesForInvariantContentItem(contentName: string, contentItemJson) {
    const contentData = await this.api.document.getByName(contentName);
    let expectedProperties = {};

    for (const property of contentData.values) {
      expectedProperties[property.alias] = property.value;
    }
    expect(contentItemJson.properties).toEqual(expectedProperties);
  }

  async verifyCulturePropertyForContentItem(contentName: string, contentItemJson, isVariesByCulture: boolean = false) {
    const contentData = await this.api.document.getByName(contentName);

    if (!isVariesByCulture) {
      expect(contentItemJson.cultures).toEqual({});
    } else {
      expect(Object.keys(contentItemJson.cultures).length).toBe(contentData.variants.length);
    }
  }

  async verifyEditorialPropertiesForContentItemWithMultiURLPicker(contentName: string, contentItemJson, pickerType: string) {
    const contentData = await this.api.document.getByName(contentName);
    const multiUrlPickerProperty = contentData.values.find((p) => p.alias === 'multiUrlPicker');
    const expectedMultiUrlPickerValue = multiUrlPickerProperty.value;
    const actualMultiUrlPickerValue = contentItemJson.properties['multiUrlPicker'];
    // Verify that the multi URL picker property has the expected structure and values  
    expect(actualMultiUrlPickerValue[0].queryString).toBe(expectedMultiUrlPickerValue[0].queryString);
    expect(actualMultiUrlPickerValue[0].title).toBe(expectedMultiUrlPickerValue[0].name);
    expect(actualMultiUrlPickerValue[0].target).toBe(expectedMultiUrlPickerValue[0].target);
    expect(actualMultiUrlPickerValue[0].destinationId).toBe(expectedMultiUrlPickerValue[0].unique);
    expect(actualMultiUrlPickerValue[0].route.path).toBe(expectedMultiUrlPickerValue[0].url);
    expect(actualMultiUrlPickerValue[0].linkType).toBe(pickerType);
  }

  async verifyPropertiesForMediaItem(mediaName: string, mediaItemJson) {
    const mediaData = await this.api.media.getByName(mediaName);

    expect(mediaItemJson.name).toBe(mediaName);
    expect(mediaItemJson.focalPoint).toEqual(mediaData.values[0].value.focalPoint);
    expect(mediaItemJson.crops).toEqual(mediaData.values[0].value.crops);
    expect(mediaItemJson.id).toEqual(mediaData.id);
    const expectedWidthValue = mediaData.values.find((p) => p.alias === 'umbracoWidth').value;
    expect(mediaItemJson.width).toEqual(Number(expectedWidthValue));
    const expectedHeightValue = mediaData.values.find((p) => p.alias === 'umbracoHeight').value;
    expect(mediaItemJson.height).toEqual(Number(expectedHeightValue));
    const expectedBytesValue = mediaData.values.find((p) => p.alias === 'umbracoBytes').value;
    expect(mediaItemJson.bytes).toEqual(Number(expectedBytesValue));
  }
}