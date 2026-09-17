import {expect} from "@playwright/test";
import {ApiHelpers} from "../ApiHelpers";
import {ConstantHelper} from "../ConstantHelper";

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

  /**
   * Polls the content query until every expected name is returned, and returns the last response body.
   *
   * Pass expectedTotal when the test asserts an exact match count: de-indexing is asynchronous, so entries
   * for documents a previous test deleted can still be counted when the new ones arrive. `take` defaults
   * well above the endpoint's own default of 10, since this decides from the page it fetched.
   *
   * The returned body is only authoritative for the page it fetched, so callers that go on to assert a name
   * is *absent* from it need that page to hold every match - which this requires, for the same reason
   * queryUntilNamesAbsent does.
   */
  async queryUntilNamesPresent(filter: string | undefined, sort: string | undefined, expectedNames: string[], expectedTotal?: number, skip = 0, take = 100) {
    expect(expectedNames.length, 'queryUntilNamesPresent needs at least one expected name, or it asserts nothing').toBeGreaterThan(0);
    let contentItemsJson;
    await expect
      .poll(
        async () => {
          const response = await this.getContentItemsFromAQuery(undefined, undefined, filter, sort, skip, take);
          contentItemsJson = await response.json();
          if (contentItemsJson.total > skip + take) {
            return [`page does not hold every match: total ${contentItemsJson.total} exceeds skip ${skip} + take ${take}`];
          }
          const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
          const missing = expectedNames.filter((name) => !returnedNames.includes(name));
          if (missing.length > 0) {
            return missing;
          }
          return expectedTotal === undefined || contentItemsJson.total === expectedTotal
            ? []
            : [`expected total ${expectedTotal}, got ${contentItemsJson.total}`];
        },
        {timeout: ConstantHelper.timeout.veryLong},
      )
      .toEqual([]);
    return contentItemsJson;
  }

  /**
   * Polls the content query until none of the given names are returned, and returns the last response body.
   *
   * Makes the absence itself the wait condition, rather than a check taken after some unrelated write has
   * landed. A name beyond the fetched page also reads as absent, which would pass silently instead of
   * timing out - hence the `take` default and the requirement that the page hold every match.
   */
  async queryUntilNamesAbsent(filter: string | undefined, sort: string | undefined, unexpectedNames: string[], skip = 0, take = 100) {
    let contentItemsJson;
    // Polls to an empty array rather than to a boolean so a failure names what was still there, or says the
    // page was too small, instead of reporting "expected true, received false".
    await expect
      .poll(
        async () => {
          const response = await this.getContentItemsFromAQuery(undefined, undefined, filter, sort, skip, take);
          contentItemsJson = await response.json();
          if (contentItemsJson.total > skip + take) {
            return [`page too small to prove absence: total ${contentItemsJson.total} exceeds skip ${skip} + take ${take}`];
          }
          const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
          return unexpectedNames.filter((name) => returnedNames.includes(name));
        },
        {timeout: ConstantHelper.timeout.veryLong},
      )
      .toEqual([]);
    return contentItemsJson;
  }

  /**
   * Polls the content query until it reports the expected total.
   *
   * Assert on total, not on items: items are index hits mapped through the published content cache, which
   * drops an unpublished document by itself - only total proves the document left the index. Paging does
   * not affect total, so this takes no skip/take.
   */
  async queryUntilTotalIs(filter: string | undefined, expectedTotal: number) {
    await expect
      .poll(
        async () => {
          const response = await this.getContentItemsFromAQuery(undefined, undefined, filter);
          return (await response.json()).total;
        },
        {timeout: ConstantHelper.timeout.veryLong},
      )
      .toBe(expectedTotal);
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