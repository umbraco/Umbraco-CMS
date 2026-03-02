import {ApiHelpers} from "./ApiHelpers";

export class MemberGroupApiHelper {
  api: ApiHelpers;

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member-group/' + id);
    return await response.json();
  }

  async create(name: string, id?: string) {
    const memberGroupData = {
      "name": name,
      "id": id ? id : null,
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/member-group', memberGroupData);
    return response.headers().location.split("v1/member-group/").pop();
  }

  async rename(id: string, name: string) {
    const memberGroupData = {
      "name": name
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/member-group/' + id, memberGroupData);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/member-group/' + id);
  }

  async getAll() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/member-group/root?skip=0&take=10000');
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member-group/' + id);
    return response.status() === 200;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getByName(name: string) {
    const rootMemberGroups = await this.getAll();
    const jsonMemberGroups = await rootMemberGroups.json();

    for (const memberGroup of jsonMemberGroups.items) {
      if (memberGroup.name === name) {
        return this.get(memberGroup.id);
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootMemberGroups = await this.getAll();
    const jsonMemberGroups = await rootMemberGroups.json();

    for (const memberGroup of jsonMemberGroups.items) {
      if (memberGroup.name === name) {
        return this.delete(memberGroup.id);
      }
    }
    return null;
  }
}