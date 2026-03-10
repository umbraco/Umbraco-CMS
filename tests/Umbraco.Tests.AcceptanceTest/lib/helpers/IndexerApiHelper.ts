import {ApiHelpers} from "./ApiHelpers";

export class IndexerApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/indexer?skip=0&take=9999');
    return await response.json(); 
  }

  async getByIndexName(indexName: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/indexer/' + indexName);
    return await response.json(); 
  }
}