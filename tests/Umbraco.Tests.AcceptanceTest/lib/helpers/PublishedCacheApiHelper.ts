import {ApiHelpers} from "./ApiHelpers";

export class PublishedCacheApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getStatus() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/published-cache/status');
    return await response.json(); 
  }
}