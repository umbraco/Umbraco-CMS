import {expect} from "@playwright/test";
import {ApiHelpers} from "../ApiHelpers";

export class MediaDeliveryApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getMediaItemWithId(id: string) {
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/media/item/' + id);
  }

  async getMediaItemWithPath(path: string) {
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/media/item' + path);
  }

  async getMediaItemsWithIds(ids: string[]) {
    let query = '?';
    for (let i = 0; i < ids.length; i++) {
      query += 'id=' + ids[i] + (i < ids.length - 1 ? '&' : ''); 
    }
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/media/items' + query);
  }

  async getMediaItemsFromAQuery(fetch: string, filter?: string, sort?: string, skip?: number, take?: number) {
    let query = '?';
    if (!fetch) {
      return null;
    }
    query += 'fetch=' + fetch;
    if (filter) {
      query += '&filter=' + filter;
    }
    if (sort) {
      query += '&sort=' + sort;
    }
    if (skip !== undefined) {
      query += '&skip=' + skip;
    }
    if (take !== undefined) {
      query += '&take=' + take;
    }
    return await this.api.get(this.api.baseUrl + '/umbraco/delivery/api/v2/media' + query);
  }

  async verifyDefaultMediaItemJson(mediaName: string, mediaItemJson, mediaPath: string, mediaTypeName: string) {
    const mediaData = await this.api.media.getByName(mediaName);

    // Verify path, createDate, updateDate, id, name, and mediaType are always included in the response
    expect(mediaItemJson.path).toBe(mediaPath);
    expect(new Date(mediaItemJson.createDate)).toEqual(new Date(mediaData.variants[0].createDate));
    expect(new Date(mediaItemJson.updateDate)).toEqual(new Date(mediaData.variants[0].createDate));  
    expect(mediaItemJson.id).toBe(mediaData.id);
    expect(mediaItemJson.name).toBe(mediaName);
    const mediaTypeData = await this.api.mediaType.getByName(mediaTypeName);
    expect(mediaItemJson.mediaType).toBe(mediaTypeData.alias);
    // Verify url, extension and the size in bytes are included for all files (not for folders).
    if (mediaTypeName !== 'Folder') {
      const mediaSrc = mediaData.values[0].value.src ? mediaData.values[0].value.src : '';
      expect(mediaItemJson.url).toBe(mediaSrc);
      const mediaExtension = mediaData.values.find(x => x.alias === 'umbracoExtension')?.value || null;
      expect(mediaItemJson.extension).toBe(mediaExtension);
      const mediaBytes = mediaData.values.find(x => x.alias === 'umbracoBytes')?.value || null;
      expect(mediaItemJson.bytes).toBe(mediaBytes ? Number(mediaBytes) : null); 
    } else {
      expect(mediaItemJson.url).toBe('');
      expect(mediaItemJson.extension).toBeNull();
      expect(mediaItemJson.bytes).toBeNull();  
    }
    // Verify width and height (in pixels), focalPoint and crops are included for most images.
    if (mediaTypeName === 'Image') {
      const mediaWidth = mediaData.values.find(x => x.alias === 'umbracoWidth')?.value;
      const mediaHeight = mediaData.values.find(x => x.alias === 'umbracoHeight')?.value;
      expect(mediaItemJson.width).toBe(mediaWidth ? Number(mediaWidth) : null);
      expect(mediaItemJson.height).toBe(mediaHeight ? Number(mediaHeight) : null);
      expect(mediaItemJson.focalPoint).toBe(mediaData.values[0].value.focalPoint);
      expect(mediaItemJson.crops).toEqual(mediaData.values[0].value.crops);
    } else {
      expect(mediaItemJson.width).toBeNull();
      expect(mediaItemJson.height).toBeNull();
      expect(mediaItemJson.focalPoint).toBeNull();
      expect(mediaItemJson.crops).toBeNull();  
    }
  }

  async verifyMultipleMediaItemsJson(mediaNames: string[], mediaItemsJson, mediaPaths: string[], mediaTypeNames: string[]) {
    expect(mediaItemsJson.length).toBe(mediaNames.length);
    for (let i = 0; i < mediaNames.length; i++) {
        await this.verifyDefaultMediaItemJson(mediaNames[i], mediaItemsJson[i], mediaPaths[i], mediaTypeNames[i]);
    }
  }

  async doesMediaItemWithIdContainValues(id: string, mediaName: string, mediaTypeName: string, url: string) {
    const mediaItem = await this.getMediaItemWithId(id);
    const mediaItemJson = await mediaItem.json();
    
    return mediaItemJson.name === mediaName && mediaItemJson.mediaType === mediaTypeName && mediaItemJson.url === url;
  }
}