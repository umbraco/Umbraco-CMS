import {ApiHelpers} from "./ApiHelpers";

export class PackageApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async ensureNameNotExists(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created?skip=0&take=10000');
    const json = await response.json();

    for (const sb of json.items) {
      if (sb.name === name) {
        if (sb.id !== null) {
          return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + sb.id);
        }
      }
    }
    return null;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + id);
    return response.status() === 200;
  }

  async doesNameExist(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created?skip=0&take=10000');
    const json = await response.json();

    if (name !== null) {
      for (const sb of json.items) {
        if (sb.name === name) {
          return true;
        }
      }
    }
    return false;
  }

  async create(name: string, contentLoadChildNodes = false, mediaLoadChildNodes = false, contentNodeId?: string, mediaIds?: string[], documentTypes?, mediaTypes?, dataTypes?, templates?, partialViews?, stylesheets?, scripts?, languages?, dictionaryItems?) {
    const packageData = {
      "name": name,
      "contentNodeId": contentNodeId,
      "contentLoadChildNodes": contentLoadChildNodes,
      "mediaIds": mediaIds,
      "mediaLoadChildNodes": mediaLoadChildNodes,
      "documentTypes": [
        documentTypes
      ],
      "mediaTypes": [
        mediaTypes
      ],
      "dataTypes": [
        dataTypes
      ],
      "templates": [
        templates
      ],
      "partialViews": [
        partialViews
      ],
      "stylesheets": [
        stylesheets
      ],
      "scripts": [
        scripts
      ],
      "languages": [
        languages
      ],
      "dictionaryItems": [
        dictionaryItems
      ]
    };

    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/package/created', packageData);
    // Returns the id of the created package
    return response.headers().location.split("/").pop();
  }

  async getByName(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created?skip=0&take=10000');
    const json = await response.json();

    if (name !== null) {
      for (const sb of json.items) {
        if (sb.name === name) {
          if (sb.id !== null) {
            const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + sb.id);
            return await response.json();
          }
        }
      }
    }
    return null;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + id);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;

  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created?skip=0&take=10000');
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async update(id: string, packageData) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + id, packageData);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + id);
  }

  async download(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/package/created/' + id + '/download');
    return await response.text();
  }

  async createEmptyPackage(name: string) {
    return await this.create(name);
  }
}