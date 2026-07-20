import {ApiHelpers} from "./ApiHelpers";

export class RelationTypeApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + id);
    return await response.json();
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + id);
    return response.status() === 200;
  }

  async create(name: string, isBidirectional: boolean, isDependency: boolean, parentObjectTypeId?: string, childObjectTypeId?: string, id?: string) {
    const relationTypeData = {
      "name": name,
      "isBidirectional": isBidirectional,
      "parentObjectType": parentObjectTypeId,
      "childObjectType": childObjectTypeId,
      "isDependency": isDependency,
      "id": id
    };

    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/relation-type', relationTypeData);
    // Returns the id of the created relationType
    return response.headers().location.split("/").pop();
  }

  async update(id: string, relationType) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + id, relationType);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + id);
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/relation-type/root?skip=0&take=10000');
  }

  async getItems(ids) {
    let idArray = 'id=' + ids[0];
    let i: number;

    for (i = 1; i < ids.length; ++i) {
      idArray += '&id=' + ids[i];
    }

    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/item?' + idArray);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async ensureNameNotExists(name: string) {
    const rootRelationType = await this.getAllAtRoot();
    const jsonRelationType = await rootRelationType.json();

    for (const relationType of jsonRelationType.items) {
      if (relationType.name === name && relationType.id !== null) {
          return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + relationType.id);
      }
    }
    return null;
  }

  async getByName(name: string) {
    const rootRelationType = await this.getAllAtRoot();
    const jsonRelationType = await rootRelationType.json();

    for (const relationType of jsonRelationType.items) {
      if (relationType.name === name && relationType.id !== null ) {
          const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/relation-type/' + relationType.id);
          return await response.json();
      }
    }
    return null;
  }

  async doesNameExist(name: string) {
    const rootRelationType = await this.getAllAtRoot();
    const jsonRelationType = await rootRelationType.json();

    if (name !== null) {
      for (const relationType of jsonRelationType.items) {
        if (relationType.name === name) {
          return true;
        }
      }
    }
    return false;
  }
}