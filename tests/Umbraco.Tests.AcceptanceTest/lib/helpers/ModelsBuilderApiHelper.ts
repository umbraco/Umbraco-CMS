import {ApiHelpers} from "./ApiHelpers";

export class ModelsBuilderApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getDashboard() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/models-builder/dashboard');
    return await response.json(); 
  }
}