import {ApiHelpers} from "./ApiHelpers";

export class ServerApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async isDebugMode() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/server/information');
    const json = await response.json();

    return json.isDebugMode;
  }
}
