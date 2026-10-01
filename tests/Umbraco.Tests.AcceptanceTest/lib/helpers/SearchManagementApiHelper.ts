import {expect} from '@playwright/test';
import {ApiHelpers} from './ApiHelpers';
import {ConstantHelper} from './ConstantHelper';

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
    const response = await this.api.get(this.api.baseUrl + `${ConstantHelper.apiEndpoints.searchIndexes}/${encodeURIComponent(indexAlias)}`);
    return await response.json();
  }

  /**
   * Runs a free-text search against an index. With a culture it matches that culture plus invariant content;
   * without one it matches invariant content only, so pass a culture when searching culture-variant documents.
   *
   * The endpoint also accepts filters/facets/sorters/segment - add `segment` here when a test needs it, but
   * not filters/facets/sorters: as of writing, sending any of those as JSON always fails with a 400.
   */
  async search(indexAlias: string, query?: string, culture?: string, skip = 0, take = 100) {
    const response = await this.searchResponse(indexAlias, query, culture, skip, take);
    return await response.json();
  }

  /**
   * Whether a search for `query` returns the document with the given id. A failed request reads as "not found"
   * rather than throwing, so this is safe to poll.
   */
  async isDocumentFound(indexAlias: string, query: string, id: string, culture?: string) {
    const response = await this.searchResponse(indexAlias, query, culture);
    if (!response.ok()) {
      return false;
    }
    const body = await response.json();
    return body.documents?.some((document: {id: string}) => document.id === id) ?? false;
  }

  /**
   * Waits until a search for `query` returns the document with the given id. Indexing is asynchronous and runs
   * behind other queued work, so this defaults to the longest timeout tier, like ApiHelpers.waitUntilItemIsIndexed.
   */
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
