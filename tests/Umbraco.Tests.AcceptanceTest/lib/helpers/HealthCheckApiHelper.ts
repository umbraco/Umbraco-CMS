import {ApiHelpers} from "./ApiHelpers";

export class HealthCheckApiHelper {
  api: ApiHelpers;

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/health-check-group?skip=0&take=9999');
    return await response.json();
  }

  async getByName(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/health-check-group/' + name);
    return await response.json();
  }

  async checkByName(name: string) {
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/health-check-group/' + name + '/check');
    return await response.json();
  }

  async getResultsCountByName(name: string) {
    const resultCount: { success: number, error: number, warning: number } = {
      success: 0,
      error: 0,
      warning: 0
    };
  
    const healthCheckData = await this.checkByName(name);
  
    healthCheckData.checks.forEach((item) => {
      item.results.forEach((result) => {
        switch (result.resultType) {
          case "Success":
            resultCount.success++;
            break;
          case "Error":
            resultCount.error++;
            break;
          case "Warning":
            resultCount.warning++;
            break;
        }
      });
    });
  
    return resultCount;
  }
}