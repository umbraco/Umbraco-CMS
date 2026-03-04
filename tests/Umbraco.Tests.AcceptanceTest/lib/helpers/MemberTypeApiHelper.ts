import {ApiHelpers} from "./ApiHelpers";
import {AliasHelper} from "./AliasHelper";
import {MemberTypeBuilder} from "../builders";

export class MemberTypeApiHelper {
  api: ApiHelpers;

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async ensureNameNotExists(name: string) {
    const rootMemberTypes = await this.getAllAtRoot();
    const jsonMemberTypes = await rootMemberTypes.json();

    for (const memberType of jsonMemberTypes.items) {
      if (memberType.name === name) {
        return await this.delete(memberType.id);
      }
    }
    return null;
  }

  async create(memberType) {
    if (memberType == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/member-type', memberType);
    return response.headers().location.split("v1/member-type/").pop();
  }

  async update(id: string, updatedMemberType) {
    if (updatedMemberType == null) {
      return;
    }
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/member-type/' + id, updatedMemberType);
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member-type/' + id);
    return await response.json();
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/member-type/' + id);
    return response.status();
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/member-type/root?skip=0&take=10000');
  }

  async getByName(name: string) {
    const rootMemberTypes = await this.getAllAtRoot();
    const jsonMemberTypes = await rootMemberTypes.json();

    for (const memberType of jsonMemberTypes.items) {
      if (memberType.name === name) {
        return this.get(memberType.id);
      }
    }
    return false;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member-type/' + id);
    return response.status() === 200;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async createDefaultMemberType(memberTypeName: string) {
    await this.ensureNameNotExists(memberTypeName);

    const memberType = new MemberTypeBuilder()
      .withName(memberTypeName)
      .withAlias(AliasHelper.toAlias(memberTypeName))
      .withAllowedAsRoot(true)
      .build();
      
    return await this.create(memberType);
  }

  async createMemberTypeWithPropertyEditor(memberTypeName: string, dataTypeName: string, dataTypeId: string, groupName: string = "GroupTest")
  {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const memberType = new MemberTypeBuilder()
      .withName(memberTypeName)
      .withAlias(AliasHelper.toAlias(memberTypeName))
      .addContainer()
        .withName(groupName)
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .done()
      .build();
    return await this.create(memberType);
  }

  async createMemberTypeWithTwoPropertyEditors(memberTypeName: string, dataTypeNameOne: string, dataTypeIdOne: string, dataTypeNameTwo: string, dataTypeIdTwo: string, groupName: string = "GroupTest")
  {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const memberType = new MemberTypeBuilder()
      .withName(memberTypeName)
      .withAlias(AliasHelper.toAlias(memberTypeName))
      .addContainer()
        .withName(groupName)
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(dataTypeNameOne))
        .withName(dataTypeNameOne)
        .withDataTypeId(dataTypeIdOne)
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(dataTypeNameTwo))
        .withName(dataTypeNameTwo)
        .withDataTypeId(dataTypeIdTwo)
        .done()
      .build();
    return await this.create(memberType);
  }
}