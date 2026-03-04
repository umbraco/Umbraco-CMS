import {ApiHelpers} from "./ApiHelpers";
import {DocumentBlueprintsBuilder} from "../builders";
import {AliasHelper} from "./AliasHelper";

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
    return response.headers().location.split("v1/document-blueprint/").pop();
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
    return items.items;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  private async recurseDeleteChildren(id: string) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.delete(id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          return await this.get(child.id);
        }
        if (child.hasChildren) {
          return await this.recurseDeleteChildren(child.id);
        } else {
          return await this.delete(child.id);
        }
      } else if (child.hasChildren) {
        await this.recurseChildren(name, child.id, toDelete);
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootDocumentBlueprints = await this.getAllAtRoot();
    const jsonDocumentBlueprints = await rootDocumentBlueprints.json();

    for (const blueprint of jsonDocumentBlueprints.items) {
      if (blueprint.name === name) {
        return this.get(blueprint.id);
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

    for (const blueprint of jsonDocumentBlueprints.items) {
      if (blueprint.name === name) {
        if (blueprint.hasChildren) {
          await this.recurseDeleteChildren(blueprint.id);
        }
        return await this.delete(blueprint.id);
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
    return response.headers().location.split("v1/document-blueprint/").pop();
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