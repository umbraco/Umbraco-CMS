import {ApiHelpers} from "./ApiHelpers";
import {ConstantHelper} from "./ConstantHelper";

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
    const response = await this.api.get(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchIndexes}/${indexAlias}`);
    return await response.json();
  }

  async rebuildIndex(indexAlias: string) {
    const response = await this.api.put(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchRebuild}?indexAlias=${indexAlias}`);
    return response.status();
  }

  async search(indexAlias: string, query?: string, skip = 0, take = 100) {
    const response = await this.api.post(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchQuery}?skip=${skip}&take=${take}`, {
      indexAlias: indexAlias,
      query: query
    });
    return await response.json();
  }
}
