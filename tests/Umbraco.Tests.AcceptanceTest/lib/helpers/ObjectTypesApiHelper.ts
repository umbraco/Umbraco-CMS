import {ApiHelpers} from "./ApiHelpers";

export class ObjectTypesApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/object-types?skip=0&take=10000');
    return await response.json(); 
  }

  async getIdByName(name: string) {
    const response = await this.getAll();
    if (response.items === null) 
      return null;
    for (const item of response.items) {
      if (item.name == name) {
        return item.id;
      }
    }
  }
}