import {ApiHelpers} from "./ApiHelpers";
import {DocumentTypeBuilder} from "../builders";
import {AliasHelper} from "./AliasHelper";

export class DocumentTypeApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async ensureNameNotExists(name: string) {
    const rootDocumentTypes = await this.getAllAtRoot();
    const jsonDocumentTypes = await rootDocumentTypes.json();

    for (const documentType of jsonDocumentTypes.items) {
      if (documentType.name === name) {
        if (documentType.isFolder) {
          return await this.recurseDeleteChildren(documentType);
        }
        return await this.delete(documentType.id);
      } else if (documentType.hasChildren) {
        await this.recurseChildren(name, documentType.id, true);
      }
    }
    return null;
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/document-type/root?skip=0&take=10000&foldersOnly=false');
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
        return await this.recurseChildren(name, child.id, toDelete);
      }
    }
    return false;
  }

  private async recurseDeleteChildren(documentTypeFolder) {
    if (!documentTypeFolder.hasChildren) {
      return await this.deleteFolder(documentTypeFolder.id);
    }
    const items = await this.getChildren(documentTypeFolder.id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else if (child.isFolder) {
        await this.deleteFolder(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.deleteFolder(documentTypeFolder.id);
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/document-type/children?parentId=${id}&skip=0&take=10000&foldersOnly=false`);
    const items = await response.json();
    return items.items;
  }

  async create(documentType) {
    if (documentType == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document-type', documentType);
    return response.headers().location.split("/").pop();
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document-type/' + id);
    const json = await response.json();
    if (json !== null) {
      return json;
    }
    return null;
  }

  async getByName(name: string) {
    const rootDocumentTypes = await this.getAllAtRoot();
    const jsonDocumentTypes = await rootDocumentTypes.json();

    for (const documentType of jsonDocumentTypes.items) {
      if (documentType.name === name) {
        if (documentType.isFolder) {
          return this.getFolder(documentType.id);
        }
        return this.get(documentType.id);
      } else if (documentType.hasChildren) {
        const result = await this.recurseChildren(name, documentType.id, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/document-type/' + id);
    return response.status();
  }

  // FOLDER
  async getFolder(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document-type/folder/' + id);
    return await response.json();
  }

  async deleteFolder(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/document-type/folder/' + id);
  }

  async createFolder(name: string, parentId?: string) {
    const folder = {
      name: name,
      parent: parentId ? {id: parentId} : null

    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document-type/folder', folder);
    return response.headers().location.split("/").pop();
  }

  async renameFolder(folderId: string, folderName: string) {
    const folder = {
      name: folderName
    }
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document-type/folder/' + folderId, folder);
  }

  async createDefaultDocumentType(documentTypeName: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditor(documentTypeName: string, dataTypeName: string, dataTypeId: string, groupName: string = "TestGroup", documentTypeVaryByCulture: boolean = false, propertyVaryByCulture: boolean = false, isMandatory: boolean = false, documentTypeVaryBySegment: boolean = false, propertyTypeVaryBySegment: boolean = false) {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
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
        .withVariesByCulture(propertyVaryByCulture)
        .withMandatory(isMandatory)
        .withVariesBySegment(propertyTypeVaryBySegment)
        .done()
      .withVariesByCulture(documentTypeVaryByCulture)
      .withVariesBySegment(documentTypeVaryBySegment)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName: string, firstPropertyName: string, firstDataTypeId: string, firstGroupName: string = "TestGroup",  secondPropertyName: string, secondDataTypeId: string, secondGroupName: string = "SecondGroup") {
    const crypto = require('crypto');
    const firstContainerId = crypto.randomUUID();
    const secondContainerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addContainer()
        .withName(firstGroupName)
        .withId(firstContainerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(firstContainerId)
        .withAlias(AliasHelper.toAlias(firstPropertyName))
        .withName(firstPropertyName)
        .withDataTypeId(firstDataTypeId)
        .done()
      .addContainer()
        .withName(secondGroupName)
        .withId(secondContainerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(secondContainerId)
        .withAlias(AliasHelper.toAlias(secondPropertyName))
        .withName(secondPropertyName)
        .withDataTypeId(secondDataTypeId)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditorInTab(documentTypeName: string, dataTypeName: string, dataTypeId: string, tabName: string, groupName: string = "TestGroup", varyByCulture: boolean = false, allowAsRoot: boolean = false) {
    const crypto = require('crypto');
    const tabId = crypto.randomUUID();
    const groupId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(allowAsRoot)
      .addContainer()
        .withName(tabName)
        .withId(tabId)
        .withType("Tab")
        .done()
      .addContainer()
        .withName(groupName)
        .withId(groupId)
        .withType("Group")
        .withParentId(tabId)
        .done()
      .addProperty()
        .withContainerId(groupId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .done()
      .withVariesByCulture(varyByCulture)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditorDirectlyInTwoTabs(documentTypeName: string, dataTypeName: string, dataTypeId: string, tabName: string, secondTabName: string, secondTabDataTypeName: string, secondTabDataTypeId: string, varyByCulture: boolean = false, allowAsRoot: boolean = false) {
    const crypto = require('crypto');
    const tabId = crypto.randomUUID();
    const secondTabId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(allowAsRoot)
      .addContainer()
        .withName(tabName)
        .withId(tabId)
        .withType("Tab")
        .withSortOrder(0)
        .done()
      .addContainer()
        .withName(secondTabName)
        .withId(secondTabId)
        .withType("Tab")
        .withSortOrder(1)
        .done()
      .addProperty()
        .withContainerId(tabId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .done()
      .addProperty()
        .withContainerId(secondTabId)
        .withAlias(AliasHelper.toAlias(secondTabDataTypeName))
        .withName(secondTabDataTypeName)
        .withDataTypeId(secondTabDataTypeId)
        .done()
      .withVariesByCulture(varyByCulture)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName: string, dataTypeName: string, dataTypeId: string, tabName: string, secondTabName: string, secondTabDataTypeName: string, secondTabDataTypeId: string, groupName: string = "TestGroup", varyByCulture: boolean = false, allowAsRoot: boolean = false) {
    const crypto = require('crypto');
    const tabId = crypto.randomUUID();
    const secondTabId = crypto.randomUUID();
    const groupId = crypto.randomUUID();
    const secondGroupId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(allowAsRoot)
      .addContainer()
        .withName(tabName)
        .withId(tabId)
        .withType("Tab")
        .withSortOrder(0)
        .done()
      .addContainer()
        .withName(secondTabName)
        .withId(secondTabId)
        .withType("Tab")
        .withSortOrder(1)
        .done()
      .addContainer()
        .withName(groupName)
        .withId(groupId)
        .withType("Group")
        .withParentId(tabId)
        .done()
      .addContainer()
        .withName(groupName + "2")
        .withId(secondGroupId)
        .withType("Group")
        .withParentId(secondTabId)
        .done()
      .addProperty()
        .withContainerId(groupId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .done()
      .addProperty()
        .withContainerId(secondGroupId)
        .withAlias(AliasHelper.toAlias(secondTabDataTypeName))
        .withName(secondTabDataTypeName)
        .withDataTypeId(secondTabDataTypeId)
        .done()
      .withVariesByCulture(varyByCulture)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithTwoPropertyEditors(documentTypeName: string, dataTypeNameOne: string, dataTypeIdOne: string, dataTypeNameTwo: string, dataTypeIdTwo: string, groupName: string = "TestGroup") {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
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
    return await this.create(documentType);
  }

  async createDefaultDocumentTypeWithAllowAsRoot(documentTypeName: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithAllowedChildNode(documentTypeName: string, allowedChildNodeId: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addAllowedDocumentType()
        .withId(allowedChildNodeId)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName: string, dataTypeName: string, dataTypeId: string, allowedChildNodeId: string, groupName: string = "TestGroup") {
    await this.ensureNameNotExists(documentTypeName);
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
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
      .addAllowedDocumentType()
        .withId(allowedChildNodeId)
        .done()
      .build();
    return await this.create(documentType);
  }


  async createDocumentTypeWithAllowedTemplate(documentTypeName: string, allowedTemplateId: string, isAllowedAsRoot:boolean = false) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(isAllowedAsRoot)
      .addAllowedTemplateId()
        .withId(allowedTemplateId)
        .done()
      .withDefaultTemplateId(allowedTemplateId)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithTwoAllowedTemplates(documentTypeName: string, allowedTemplateOneId: string, allowedTemplateTwoId: string, isAllowedAsRoot: boolean = false, defaultTemplateId?: string) {
    await this.ensureNameNotExists(documentTypeName);
    const defaultTemplate = defaultTemplateId === undefined ? allowedTemplateOneId : defaultTemplateId;

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(isAllowedAsRoot)
      .addAllowedTemplateId()
        .withId(allowedTemplateOneId)
        .done()
      .addAllowedTemplateId()
        .withId(allowedTemplateTwoId)
        .done()
      .withDefaultTemplateId(defaultTemplate)
      .build();
    return await this.create(documentType);
  }
  
  async createDocumentTypeWithTwoGroups(documentTypeName: string,dataType: string, dataTypeId: string, groupNameOne: string, groupNameTwo: string) {
    const crypto = require('crypto');
    const groupOneId = crypto.randomUUID();
    const groupTwoId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .addContainer()
        .withName(groupNameOne)
        .withId(groupOneId)
        .withType("Group")
        .withSortOrder(0)
        .done()
      .addContainer()
        .withName(groupNameTwo)
        .withId(groupTwoId)
        .withType("Group")
        .withSortOrder(1)
        .done()
      .addProperty()
        .withContainerId(groupOneId)
        .withAlias(AliasHelper.toAlias(dataType + "One"))
        .withName(dataType + "One")
        .withDataTypeId(dataTypeId)
        .done()
      .addProperty()
        .withContainerId(groupTwoId)
        .withAlias(AliasHelper.toAlias(dataType + "Two"))
        .withName(dataType + "Two")
        .withDataTypeId(dataTypeId)
        .done()
      .build();
    return await this.create(documentType);
  }
  
  async createDocumentTypeWithAComposition(documentTypeName: string, compositionId: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .addComposition()
        .withDocumentTypeId(compositionId)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createEmptyElementType(elementTypeName: string) {
    await this.ensureNameNotExists(elementTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(elementTypeName)
      .withAlias(AliasHelper.toAlias(elementTypeName))
      .withIsElement(true)
      .withIcon("icon-plugin")
      .build();
    return await this.create(documentType);
  }
  
  async createDocumentTypeWithTwoTabs(documentTypeName: string, dataType: string, dataTypeId: string, tabNameOne: string, tabNameTwo: string) {
    const crypto = require('crypto');
    const tabOneId = crypto.randomUUID();
    const tabTwoId = crypto.randomUUID();
    const groupOneId = crypto.randomUUID();
    const groupTwoId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .addContainer()
        .withName(tabNameOne)
        .withId(tabOneId)
        .withType("Tab")
        .withSortOrder(0)
        .done()
      .addContainer()
        .withName(tabNameTwo)
        .withId(tabTwoId)
        .withType("Tab")
        .withSortOrder(1)
        .done()
      .addContainer()
        .withName("GroupTestOne")
        .withId(groupOneId)
        .withType("Group")
        .withParentId(tabOneId)
        .done()
      .addProperty()
        .withContainerId(groupOneId)
        .withAlias(AliasHelper.toAlias(dataType + "One"))
        .withName(dataType + "One")
        .withDataTypeId(dataTypeId)
        .done()
      .addContainer()
        .withName("GroupTestTwo")
        .withId(groupTwoId)
        .withType("Group")
        .withParentId(tabTwoId)
        .done()
      .addProperty()
        .withContainerId(groupTwoId)
        .withAlias(AliasHelper.toAlias(dataType + "Two"))
        .withName(dataType + "Two")
        .withDataTypeId(dataTypeId)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createDefaultElementType(elementName: string, groupName: string = 'TestGroup', dataTypeName: string = 'Textstring', dataTypeId: string, isMandatory: boolean = false) {
    await this.ensureNameNotExists(elementName);
    
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    
    const documentType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(AliasHelper.toAlias(elementName))
      .withIsElement(true)
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
        .withMandatory(isMandatory)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createDefaultElementTypeWithVaryByCulture(elementName: string, groupName: string = 'TestGroup', dataTypeName: string = 'Textstring', dataTypeId: string, elementTypeVaryByCulture: boolean, dataTypeVaryByCulture: boolean) {
    await this.ensureNameNotExists(elementName);

    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const documentType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(AliasHelper.toAlias(elementName))
      .withIsElement(true)
      .withVariesByCulture(elementTypeVaryByCulture)
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
        .withVariesByCulture(dataTypeVaryByCulture)
      .done()
      .build();
    return await this.create(documentType);
  }
  
  async createElementTypeWithRegexValidation(elementName: string, groupName: string = 'TestGroup', dataTypeName: string = 'Textstring', dataTypeId: string, regex: string) {
    await this.ensureNameNotExists(elementName);

    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const documentType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(AliasHelper.toAlias(elementName))
      .withIsElement(true)
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
        .withRegEx(regex)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createElementTypeWithPropertyInTab(elementName: string, tabName: string = 'ContentTab', groupName: string = 'TestGroup', dataTypeName: string = 'Textstring', dataTypeId: string, isMandatory: boolean = false) {
    await this.ensureNameNotExists(elementName);

    const crypto = require('crypto');
    const tabId = crypto.randomUUID();
    const groupId = crypto.randomUUID();

    const documentType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(AliasHelper.toAlias(elementName))
      .withIsElement(true)
      .withIcon("icon-plugin")
      .addContainer()
        .withName(tabName)
        .withId(tabId)
        .withType("Tab")
        .done()
      .addContainer()
        .withName(groupName)
        .withId(groupId)
        .withType("Group")
        .withParentId(tabId)
        .done()
      .addProperty()
        .withContainerId(groupId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .withMandatory(isMandatory)
        .done()
      .build();
    return await this.create(documentType);
  }
  
  async doesGroupContainCorrectPropertyEditor(documentTypeName: string, dataTypeName: string, dataTypeId: string, groupName: string) {
    const documentType = await this.getByName(documentTypeName);
    const group = documentType.containers.find(x => x.name === groupName);
    // Check if group is defined
    if (group) {
      // Check if the document type properties include the specified property, and it belongs to the group
      return documentType.properties.find(x => x.name === dataTypeName && x.dataType.id === dataTypeId && x.container.id === group.id);
    } else {
      // Group not found
      return false;
    }
  }

  async doesTabContainCorrectPropertyEditorInGroup(documentTypeName: string, dataTypeName: string, dataTypeId: string, tabName: string, groupName: string) {
    const documentType = await this.getByName(documentTypeName);
    const tab = documentType.containers.find(x => x.name === tabName);
    // Check if tab is defined
    if (tab) {
      const group = documentType.containers.find(x => x.name === groupName && x.parent.id === tab.id);
      // Check if group is defined
      if (group) {
        // Check if the document type properties include the specified property, and it belongs to the group
        return documentType.properties.find(x => x.name === dataTypeName && x.dataType.id === dataTypeId && x.container.id === group.id);
      } else {
        // Group not found
        return false;
      }
    } else {
      // Tab not found
      return false;
    }
  }
  
  async doesDocumentTypeGroupNameContainCorrectSortOrder(documentTypeName: string, groupName: string, sortOrder: number) {
    const documentType = await this.getByName(documentTypeName);
    const group = documentType.containers.find(x => x.name === groupName);
    // Check if group is defined
    if (group) {
      return group.sortOrder === sortOrder;
    } else {
      // Group not found
      return false;
    }
  }
  
  async doesDocumentTypeTabNameContainCorrectSortOrder(documentTypeName: string, tabName: string, sortOrder: number) {
    const documentType = await this.getByName(documentTypeName);
    const tab = documentType.containers.find(x => x.name === tabName);
    // Check if tab is defined
    if (tab) {
      return tab.sortOrder === sortOrder;
    } else {
      // Tab not found
      return false;
    }
  }
  
  async getContainerIdWithName(documentTypeName: string, containerName: string) {
    const documentType = await this.getByName(documentTypeName);
    const container = documentType.containers.find(x => x.name === containerName);
    if (container) {
      return container.id;
    } else {
      return null;
    }
  }

  async createDocumentTypeWithAllowedTwoChildNodes(documentTypeName: string, allowedChildNodeOneId: string, allowedChildNodeTwoId: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addAllowedDocumentType()
        .withId(allowedChildNodeOneId)
        .done()
      .addAllowedDocumentType()
        .withId(allowedChildNodeTwoId)
        .done()
      .build();
    return await this.create(documentType);
  }
  
  async createDocumentTypeWithAllowedChildNodeAndDataType(documentTypeName: string, allowedChildNodeId: string, dataTypeName: string, dataTypeId: string, groupName: string = "TestGroup") {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
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
      .addAllowedDocumentType()
        .withId(allowedChildNodeId)
        .done()
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName: string, allowedChildNodeId: string, collectionId: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addAllowedDocumentType()
        .withId(allowedChildNodeId)
        .done()
      .withCollectionId(collectionId)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithCollectionId(documentTypeName: string, collectionId: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .withCollectionId(collectionId)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithAllowVaryByCulture(documentTypeName: string) {
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .withVariesByCulture(true)
      .build();
    return await this.create(documentType);
  }

  async createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName: string, dataTypeId: string, propertyName: string, templateId: string) {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addContainer()
        .withName('TestGroup')
        .withId(containerId)
        .withType('Group')
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(propertyName))
        .withName(propertyName)
        .withDataTypeId(dataTypeId)
        .done()
      .addAllowedTemplateId()
        .withId(templateId)
        .done()
      .withDefaultTemplateId(templateId)
      .build();
    
    return await this.create(documentType);
  }

  async createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName: string, dataTypeName: string, dataTypeId: string) {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .addContainer()
        .withName('TestGroup')
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withName(dataTypeName)
        .withDataTypeId(dataTypeId)
        .withVariesByCulture(false)
        .done()
      .withVariesByCulture(true)
      .build();

    return await this.create(documentType);
  }

  async createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName: string, allowedChildNodeId: string, dataTypeName: string, dataTypeId: string, groupName: string = "TestGroup") {
    const crypto = require('crypto');
    const containerId = crypto.randomUUID();
    await this.ensureNameNotExists(documentTypeName);

    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
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
        .withVariesByCulture(false)
        .done()
      .addAllowedDocumentType()
        .withId(allowedChildNodeId)
        .done()
      .withVariesByCulture(true)
      .build();
      
    return await this.create(documentType);
  }

  async getPropertyIdWithName(documentTypeId: string, propertyName: string) {
    const documentTypeData = await this.get(documentTypeId);
    const property = documentTypeData.properties.find(x => x.name === propertyName);
    if (property) {
      return property.id;
    } else {
      return null;
    }
  }

  async createElementTypeWithTwoPropertyEditors(elementTypeName: string, firstDataTypeName: string, firstDataTypeId: string, secondDataTypeName: string, secondDataTypeId: string, elementTypeVaryByCulture: boolean = false, firstPropertyVaryByCulture: boolean = false, secondPropertyVaryByCulture: boolean = false) {
    const crypto = require('crypto');
    await this.ensureNameNotExists(elementTypeName);

    const containerId = crypto.randomUUID();
    const elementType = new DocumentTypeBuilder()
      .withName(elementTypeName)
      .withAlias(AliasHelper.toAlias(elementTypeName))
      .withIsElement(true)
      .withVariesByCulture(elementTypeVaryByCulture)
      .withIcon("icon-plugin")
      .addContainer()
        .withName('Content')
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(firstDataTypeName))
        .withName(firstDataTypeName)
        .withDataTypeId(firstDataTypeId)
        .withVariesByCulture(firstPropertyVaryByCulture)
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(secondDataTypeName))
        .withName(secondDataTypeName)
        .withDataTypeId(secondDataTypeId)
        .withVariesByCulture(secondPropertyVaryByCulture)
        .done()
      .build();

    return await this.create(elementType);
  }

  /**
   * Creates a variant element type with two properties: one variant and one invariant.
   * Useful for testing scenarios where a block has both culture-specific and shared properties.
   */
  async createVariantElementTypeWithVariantAndInvariantProperty(elementName: string, groupName: string, variantPropertyName: string, invariantPropertyName: string, dataTypeId: string) {
    await this.ensureNameNotExists(elementName);

    const crypto = require('crypto');
    const containerId = crypto.randomUUID();

    const documentType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(AliasHelper.toAlias(elementName))
      .withIsElement(true)
      .withVariesByCulture(true)  // Element varies by culture
      .addContainer()
        .withName(groupName)
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(variantPropertyName))
        .withName(variantPropertyName)
        .withDataTypeId(dataTypeId)
        .withVariesByCulture(true)  // This property varies by culture
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(invariantPropertyName))
        .withName(invariantPropertyName)
        .withDataTypeId(dataTypeId)
        .withVariesByCulture(false)  // This property does NOT vary by culture
        .done()
      .build();

    return await this.create(documentType);
  }

  /**
   * Creates a document type with variant and invariant block lists for testing multilingual scenarios.
   *
   * Structure created:
   * - Document Type (Vary by culture)
   *   - Text 1 (Vary by culture)
   *   - Text 2 (Shared)
   *   - Block List 1 (Vary by culture) - contains Block 1 and Block 2
   *   - Block List 2 (Shared) - contains Block 1 and Block 2
   *
   * - Block 1 Element Type (Vary by culture)
   *   - Text 1 (Vary by culture)
   *   - Text 2 (Shared)
   *
   * - Block 2 Element Type (Shared/Invariant)
   *   - Text 1
   *   - Text 2
   */
  async createDocumentTypeWithVariantAndInvariantBlockLists(documentTypeName: string, firstDataTypeName: string, firstDataTypeId: string, secondDataTypeName: string, secondDataTypeId: string, blockList1DataTypeName: string, blockList2DataTypeName: string, block1ElementTypeName: string, block2ElementTypeName: string) {
    const crypto = require('crypto');
    await this.ensureNameNotExists(documentTypeName);

    // Create Block 1 Element Type (Vary by culture) with first property (vary by culture) and second property (shared)
    const block1ElementTypeId = await this.createElementTypeWithTwoPropertyEditors(block1ElementTypeName, firstDataTypeName, firstDataTypeId, secondDataTypeName, secondDataTypeId, true, true, false) as string;

    // Create Block 2 Element Type (Shared/Invariant) with first property and second property
    const block2ElementTypeId = await this.createElementTypeWithTwoPropertyEditors(block2ElementTypeName, firstDataTypeName, firstDataTypeId, secondDataTypeName, secondDataTypeId, false, false, false) as string;

    // Create Block List 1 Data Type (will be used with vary by culture property)
    const blockList1DataTypeId = await this.api.dataType.createBlockListDataTypeWithTwoBlocks(blockList1DataTypeName, block1ElementTypeId, block2ElementTypeId) as string;

    // Create Block List 2 Data Type (will be used with shared property)
    const blockList2DataTypeId = await this.api.dataType.createBlockListDataTypeWithTwoBlocks(blockList2DataTypeName, block1ElementTypeId, block2ElementTypeId) as string;

    // Create Document Type (Vary by culture)
    const containerId = crypto.randomUUID();
    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(AliasHelper.toAlias(documentTypeName))
      .withAllowedAsRoot(true)
      .withVariesByCulture(true)
      .addContainer()
        .withName('Content')
        .withId(containerId)
        .withType("Group")
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(firstDataTypeName))
        .withName(firstDataTypeName)
        .withDataTypeId(firstDataTypeId)
        .withVariesByCulture(true)
        .withSortOrder(0)
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(secondDataTypeName))
        .withName(secondDataTypeName)
        .withDataTypeId(secondDataTypeId)
        .withVariesByCulture(false)
        .withSortOrder(1)
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(blockList1DataTypeName))
        .withName(blockList1DataTypeName)
        .withDataTypeId(blockList1DataTypeId)
        .withVariesByCulture(true)
        .withSortOrder(2)
        .done()
      .addProperty()
        .withContainerId(containerId)
        .withAlias(AliasHelper.toAlias(blockList2DataTypeName))
        .withName(blockList2DataTypeName)
        .withDataTypeId(blockList2DataTypeId)
        .withVariesByCulture(false)
        .withSortOrder(3)
        .done()
      .build();

    return await this.create(documentType);
  }
}