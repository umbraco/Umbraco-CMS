import {ApiHelpers} from "./ApiHelpers";

export class RedirectManagementApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management/' + id);
    return await response.json(); 
  }

  /**
   * @returns the raw {@link APIResponse}, not parsed JSON — the endpoint responds 200 with no body,
   * and parsing that as JSON throws "Unexpected end of JSON input".
   */
  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management/' + id);
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management?skip=0&take=1000');
    return await response.json();
  }

  // Redirects survive content deletion by design, so tests asserting on redirect counts must clear these explicitly.
  async deleteAllRedirects() {
    for (const redirect of this.api.itemsOf(await this.getAll())) {
      await this.delete(redirect.id);
    }
  }

  async getStatus() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management/status');
    return await response.json(); 
  }

  async setStatus(status: string) {
    return await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management/status?status=' + status);
  }

  async filterByText(text: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management?filter=' + text + '&skip=0&take=1000');
    return await response.json(); 
  }
}