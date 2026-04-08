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
        if (memberType.isFolder) {
          return await this.recurseDeleteChildren(memberType);
        }
        return await this.delete(memberType.id);
      } else if (memberType.hasChildren) {
        await this.recurseChildren(name, memberType.id, true);
      }
    }
    return null;
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          if (child.isFolder) {
            return await this.getFolder(child.id);
          }
          return await this.get(child.id);
        }
        if (child.isFolder) {
          return await this.recurseDeleteChildren(child);
        }
        return await this.delete(child.id);
      } else if (child.hasChildren) {
        const result = await this.recurseChildren(name, child.id, toDelete);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  private async recurseDeleteChildren(memberTypeFolder) {
    if (!memberTypeFolder.hasChildren) {
      return await this.deleteFolder(memberTypeFolder.id);
    }
    const items = await this.getChildren(memberTypeFolder.id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else if (child.isFolder) {
        await this.deleteFolder(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.deleteFolder(memberTypeFolder.id);
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
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/member-type/root?skip=0&take=10000&foldersOnly=false');
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/member-type/children?parentId=${id}&skip=0&take=10000&foldersOnly=false`);
    const items = await response.json();
    return items.items;
  }

  async getByName(name: string) {
    const rootMemberTypes = await this.getAllAtRoot();
    const jsonMemberTypes = await rootMemberTypes.json();

    for (const memberType of jsonMemberTypes.items) {
      if (memberType.name === name) {
        if (memberType.isFolder) {
          return await this.getFolder(memberType.id);
        }
        return await this.get(memberType.id);
      } else if (memberType.isContainer || memberType.hasChildren) {
        const result = await this.recurseChildren(name, memberType.id, false);
        if (result) {
          return result;
        }
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

  async createMemberTypeWithAComposition(memberTypeName: string, compositionId: string) {
    await this.ensureNameNotExists(memberTypeName);

    const memberType = new MemberTypeBuilder()
      .withName(memberTypeName)
      .withAlias(AliasHelper.toAlias(memberTypeName))
      .addComposition()
        .withMemberTypeId(compositionId)
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

  // Folder
  async getFolder(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member-type/folder/' + id);
    return await response.json();
  }

  async deleteFolder(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/member-type/folder/' + id);
  }

  async createFolder(name: string, parentId?: string) {
    const folder = {
      "name": name,
      "parent": parentId ? {"id": parentId} : null
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/member-type/folder', folder);
    return response.headers().location.split("/").pop();
  }

  async renameFolder(folderId: string, folderName: string) {
    const folder = {
      "name": folderName
    }
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/member-type/folder/' + folderId, folder);
  }
}
