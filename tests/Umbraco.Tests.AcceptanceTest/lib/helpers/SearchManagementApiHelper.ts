import {expect} from '@playwright/test';
import {ApiHelpers} from './ApiHelpers';
import {ConstantHelper} from './ConstantHelper';

export class SearchManagementApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAllIndexes() {
    const response = await this.api.get(this.api.baseUrl + ConstantHelper.apiEndpoints.searchIndexes);
    return await response.json();
  }

  async getIndex(indexAlias: string) {
    const response = await this.api.get(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchIndexes}/${encodeURIComponent(indexAlias)}`);
    return await response.json();
  }

  async search(indexAlias: string, query?: string, culture?: string, skip = 0, take = 100) {
    const response = await this.searchResponse(indexAlias, query, culture, skip, take);
    return await response.json();
  }

  async isDocumentFound(indexAlias: string, query: string, id: string, culture?: string) {
    const response = await this.searchResponse(indexAlias, query, culture);
    if (!response.ok()) {
      return false;
    }
    const body = await response.json();
    return body.documents?.some((document: {id: string}) => document.id === id) ?? false;
  }

  async waitUntilDocumentIsFound(indexAlias: string, query: string, id: string, culture?: string, timeout: number = ConstantHelper.timeout.pageLoad) {
    await expect.poll(async () => this.isDocumentFound(indexAlias, query, id, culture), {timeout: timeout}).toBeTruthy();
  }

  private async searchResponse(indexAlias: string, query?: string, culture?: string, skip = 0, take = 100) {
    return await this.api.post(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchQuery}?skip=${skip}&take=${take}`, {
      indexAlias: indexAlias,
      query: query,
      culture: culture
    });
  }
}
