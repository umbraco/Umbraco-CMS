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

  async delete(id: string) {
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management/' + id);
    return await response.json(); 
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/redirect-management?skip=0&take=1000');
    return await response.json(); 
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