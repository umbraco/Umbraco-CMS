import {ApiHelpers} from "./ApiHelpers";

export class LogViewerApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getLevel() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/level?skip=0&take=1000');
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getLevelCount(startDate = null, endDate = null) {
    const queryParams: string[] = [];

    if (startDate !== null) {
      queryParams.push('startDate=' + startDate);
    }

    if (endDate !== null) {
      queryParams.push('endDate=' + endDate);
    }

    const query = queryParams.length > 0 ? '?' + queryParams.join('&') : '';
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/level-count' + query);

    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getLog(skip = 0, take = 100, orderDirection: string, filterExpression = null, logLevel = null, startDate = null, endDate = null) {
    const queryParams = [
      filterExpression !== null ? `&filterExpression=${filterExpression}` : '',
      logLevel !== null ? `&logLevel=${logLevel}` : '',
      startDate !== null ? `&startDate=${startDate}` : '',
      endDate !== null ? `&endDate=${endDate}` : '',
    ].join('');
    const response = await this.api.get(this.api.baseUrl + `/umbraco/management/api/v1/log-viewer/log?skip=${skip}&take=${take}&orderDirection=${orderDirection}` + queryParams);

    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getMessageTemplate(skip = 0, take = 100, startDate = null, endDate = null) {
    const queryParams = [
      startDate !== null ? `&startDate=${startDate}` : '',
      endDate !== null ? `&endDate=${endDate}` : '',
    ].join('');
    const response = await this.api.get(this.api.baseUrl + `/umbraco/management/api/v1/log-viewer/message-template?skip=${skip}&take=${take}` + queryParams);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getSavedSearches(skip = 0, take = 100) {
    const response = await this.api.get(this.api.baseUrl + `/umbraco/management/api/v1/log-viewer/saved-search?skip=${skip}&take=${take}`);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async createSavedSearch(name: string, query: string) {
    const searchData = {
      "name": name,
      "query": query
    }

    const response = await this.api.post(this.api.baseUrl + `/umbraco/management/api/v1/log-viewer/saved-search`, searchData);
    return response.headers().location.split("/").pop();
  }

  async getSavedSearch(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/saved-search/' + name);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async deleteSavedSearch(name: string) {
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/saved-search/' + name);
    return response.status();
  }

  async validateLogSize(startDate = null, endDate = null) {
    const queryParams: string[] = [];

    if (startDate !== null) {
      queryParams.push('startDate=' + startDate);
    }

    if (endDate !== null) {
      queryParams.push('endDate=' + endDate);
    }

    const query = queryParams.length > 0 ? '?' + queryParams.join('&') : '';
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/validate-logs-size' + query);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async doesSavedSearchExist(name: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/log-viewer/saved-search/' + name);
    if (response.status() === 404) {
      return false; // Not found, the saved search does not exist
    }

    return response.status() === 200; // Check if the response status is 200 (OK)
  }
}
