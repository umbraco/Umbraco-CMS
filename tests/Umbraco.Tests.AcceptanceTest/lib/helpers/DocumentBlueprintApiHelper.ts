import {ApiHelpers} from "./ApiHelpers";
import {AliasHelper} from "./AliasHelper";
import {DocumentBlueprintsBuilder} from "../builders";

// The fields of a tree item this helper needs in order to delete it and recurse into its children.
type DocumentBlueprintTreeItem = {
  id: string;
  hasChildren?: boolean;
  isFolder?: boolean;
};

export class DocumentBlueprintApiHelper {
  api: ApiHelpers;

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/' + id);
    return await response.json();
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/' + id);
    return response.status() === 200;
  }

  async create(documentBlueprint) {
    if (documentBlueprint == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint', documentBlueprint);
    return this.api.getIdFromLocation(response);
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/' + id);
    return response.status();
  }

  async update(id: string, documentBlueprint) {
    if (documentBlueprint == null) {
      return;
    }
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/' + id, documentBlueprint);
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/document-blueprint/root?skip=0&take=1000&foldersOnly=false');
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/document-blueprint/children?parentId=${id}&skip=0&take=10000&foldersOnly=false`);
    const items = await response.json();
    return this.api.itemsOf(items);
  }

  async createFolder(name: string, parentId?: string) {
    const folder = {
      name: name,
      parent: parentId ? {id: parentId} : null
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/folder', folder);
    return this.api.getIdFromLocation(response);
  }

  async getFolder(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/folder/' + id);
    return await response.json();
  }

  async deleteFolder(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/folder/' + id);
  }

  async doesFolderExist(id: string) {
    const folder = await this.getFolder(id);
    return folder !== null && folder.id === id;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  private async recurseDeleteItem(item: DocumentBlueprintTreeItem) {
    if (item.hasChildren) {
      const items = await this.getChildren(item.id);

      for (const child of items) {
        await this.recurseDeleteItem(child);
      }
    }

    // A folder has to be empty before it can be deleted, so its children are removed first.
    return item.isFolder ? await this.deleteFolder(item.id) : await this.delete(item.id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          return child.isFolder ? await this.getFolder(child.id) : await this.get(child.id);
        }
        return await this.recurseDeleteItem(child);
      } else if (child.hasChildren) {
        const result = await this.recurseChildren(name, child.id, toDelete);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootDocumentBlueprints = await this.getAllAtRoot();
    const jsonDocumentBlueprints = await rootDocumentBlueprints.json();

    for (const blueprint of this.api.itemsOf(jsonDocumentBlueprints)) {
      if (blueprint.name === name) {
        return blueprint.isFolder ? this.getFolder(blueprint.id) : this.get(blueprint.id);
      } else if (blueprint.hasChildren) {
        const result = await this.recurseChildren(name, blueprint.id, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootDocumentBlueprints = await this.getAllAtRoot();
    const jsonDocumentBlueprints = await rootDocumentBlueprints.json();

    for (const blueprint of this.api.itemsOf(jsonDocumentBlueprints)) {
      if (blueprint.name === name) {
        return await this.recurseDeleteItem(blueprint);
      } else if (blueprint.hasChildren) {
        await this.recurseChildren(name, blueprint.id, true);
      }
    }
    return null;
  }

  async createDefaultDocumentBlueprint(documentBlueprintName: string, documentTypeId: string) {
    await this.ensureNameNotExists(documentBlueprintName);
    const documentBlueprint = new DocumentBlueprintsBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentBlueprintName)
        .done()
      .build();
    return await this.create(documentBlueprint);
  }

  async createFromDocument(documentBlueprintName: string, documentId: string, parentId?: string) {
    const documentBlueprintData = {
      "name": documentBlueprintName,
      "parent": parentId ? parentId : null,
      "document": {
        "id": documentId
      }
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document-blueprint/from-document', documentBlueprintData);
    return this.api.getIdFromLocation(response);
  }

  async createDocumentBlueprintWithTextBoxValue(documentBlueprintName: string, documentTypeId: string, dataTypeName: string, text: string) {
    await this.ensureNameNotExists(documentBlueprintName);

    const documentBlueprint = new DocumentBlueprintsBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentBlueprintName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(text)
        .withEditorAlias('Umbraco.TextBox')
        .done()
      .build();

    return await this.create(documentBlueprint);
  }

  async createDefaultDocumentBlueprintWithABlockListEditorAndBlockWithValue(documentBlueprintName: string, documentTypeName: string, blockListDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyEditorAlias: string, elementTypePropertyValue: string, groupName: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, groupName) || '';
    await this.ensureNameNotExists(documentBlueprintName);

    const documentBlueprint = new DocumentBlueprintsBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentBlueprintName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .withEditorAlias('Umbraco.BlockList')
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();

    return await this.create(documentBlueprint);
  }

  async createDefaultDocumentBlueprintWithABlockGridEditorAndBlockWithValue(documentBlueprintName: string, documentTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyEditorAlias: string, elementTypePropertyValue: string, groupName: string = 'TestGroup') {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId, true) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName) || '';
    await this.ensureNameNotExists(documentBlueprintName);

    const documentBlueprint = new DocumentBlueprintsBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentBlueprintName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .withEditorAlias('Umbraco.BlockGrid')
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();

    return await this.create(documentBlueprint);
  }

  async createDocumenBlueprintWithEnglishCultureAndDanishCultureAndTextBoxValue(documentBlueprintEnglishName: string, documentBlueprintDanishName: string, documentTypeId: string, dataTypeName: string, textContent: string, varyByCultureForText: boolean = false) {
    await this.ensureNameNotExists(documentBlueprintEnglishName);
    const cultureValue = varyByCultureForText ? 'en-US' : null;

    const documentBlueprint = new DocumentBlueprintsBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentBlueprintEnglishName)
        .withCulture('en-US')
        .done()
      .addVariant()
        .withName(documentBlueprintDanishName)
        .withCulture('da')
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .withCulture(cultureValue)
        .withEditorAlias('Umbraco.TextBox')
        .done()
      .build();

    return await this.create(documentBlueprint);
  }
}
