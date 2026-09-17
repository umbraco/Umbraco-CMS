import {ApiHelpers} from "./ApiHelpers";
import {ConstantHelper} from "./ConstantHelper";

/**
 * Wraps the Search Management API (`/umbraco/search/api/v1`), which exposes the search indexes and the
 * ad-hoc search the back office's search box runs.
 */
export class SearchManagementApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  /** Gets every registered index, with its provider, document count and health status. */
  async getAllIndexes() {
    const response = await this.api.get(this.api.baseUrl + ConstantHelper.apiEndpoints.searchIndexes);
    return await response.json();
  }

  /** Gets a single index by alias. Poll this to observe indexing catching up with a write. */
  async getIndex(indexAlias: string) {
    const response = await this.api.get(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchIndexes}/${indexAlias}`);
    return await response.json();
  }

  /** Triggers a rebuild and returns the response status. The rebuild itself runs in the background. */
  async rebuildIndex(indexAlias: string) {
    const response = await this.api.put(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchRebuild}?indexAlias=${indexAlias}`);
    return response.status();
  }

  /**
   * Runs a free-text search against an index.
   *
   * The endpoint also accepts filters, facets, sorters, culture and segment; add them here as parameters
   * when a test needs them.
   */
  async search(indexAlias: string, query?: string, skip = 0, take = 100) {
    const response = await this.api.post(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchQuery}?skip=${skip}&take=${take}`, {
      indexAlias: indexAlias,
      query: query
    });
    return await response.json();
  }
}
