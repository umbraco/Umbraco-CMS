import {ApiHelpers} from "./ApiHelpers";

export class TelemetryDataApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async setLevel(level: string) {
    const data = {
      "telemetryLevel": level
    }
    return await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/telemetry/level', data);
  }

  async getLevel() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/telemetry/level');
    const json = await response.json();

    return json.telemetryLevel;
  }
}