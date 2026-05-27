import {AliasHelper} from "./AliasHelper";
import {ApiHelpers} from "./ApiHelpers";
import {ConstantHelper} from "./ConstantHelper";
import {ElementBuilder} from "../builders";

export class ElementApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}/${id}`);
    return await response.json();
  }

  async doesExist(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}/${id}`);
    return response.status() === 200;
  }

  async create(element: any) {
    if (element == null) {
      return;
    }
    const response = await this.api.post(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}`, element);
    return response.headers().location.split("v1/element/").pop();
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}/${id}`);
    return response.status();
  }

  async update(id: string, element) {
    if (element == null) {
      return;
    }
    const variantsData = element.variants.map(variant => ({
      culture: variant.culture,
      segment: variant.segment,
      name: variant.name
    }));

    const updateData = {
      values: element.values,
      variants: variantsData
    };
    return await this.api.put(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}/${id}`, updateData);
  }

  async getAllAtRoot() {
    return await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.treeElementRoot}?skip=0&take=10000&foldersOnly=false`);
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.treeElementChildren}?parentId=${id}&skip=0&take=10000`);
    const items = await response.json();
    return items.items;
  }

  async getChildrenAmount(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.treeElementChildren}?parentId=${id}&skip=0&take=10000`);
    const items = await response.json();
    return items.total;
  }

  async doesNameExist(name: string) {
    const result = await this.getByName(name);
    return result !== false;
  }

  private async recurseDeleteChildren(id: string, isFolder: boolean = false) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child.id, child.isFolder);
      } else {
        child.isFolder ? await this.deleteFolder(child.id) : await this.delete(child.id);
      }
    }
    // Delete parent using the correct method
    return isFolder ? await this.deleteFolder(id) : await this.delete(id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      // Use root level 'name' property for both folders and elements
      if (child.name === name) {
        if (!toDelete) {
          return child.isFolder ? await this.getFolder(child.id) : await this.get(child.id);
        }
        if (child.hasChildren) {
          return await this.recurseDeleteChildren(child.id, child.isFolder);
        } else {
          return child.isFolder ? await this.deleteFolder(child.id) : await this.delete(child.id);
        }
      }

      if (child.hasChildren) {
        const result = await this.recurseChildren(name, child.id, toDelete);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootElements = await this.getAllAtRoot();
    const jsonElements = await rootElements.json();

    for (const element of jsonElements.items) {
      // Use root level 'name' property for both folders and elements
      if (element.name === name) {
        return element.isFolder ? await this.getFolder(element.id) : await this.get(element.id);
      }

      // Recursively search children
      if (element.hasChildren) {
        const result = await this.recurseChildren(name, element.id, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootElements = await this.getAllAtRoot();
    const jsonElements = await rootElements.json();

    for (const element of jsonElements.items) {
      // Use root level 'name' property for both folders and elements
      if (element.name === name) {
        if (element.hasChildren) {
          await this.recurseDeleteChildren(element.id, element.isFolder);
        } else {
          element.isFolder ? await this.deleteFolder(element.id) : await this.delete(element.id);
        }
      } else if (element.hasChildren) {
        // Search in children
        await this.recurseChildren(name, element.id, true);
      }
    }
  }

  async publish(id: string, publishSchedulesData: any = {"publishSchedules": [{"culture": null}]}) {
    if (id == null) {
      return;
    }
    const response = await this.api.put(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.element}/${id}/publish`, publishSchedulesData);
    return response.status();
  }

  async moveToRecycleBin(id: string, isFolder: boolean = false) {
    if (id == null) {
      return;
    }
    const endpoint = isFolder ? ConstantHelper.apiEndpoints.elementFolder : ConstantHelper.apiEndpoints.element;
    const response = await this.api.put(`${this.api.baseUrl}${endpoint}/${id}/move-to-recycle-bin`);
    return response.status();
  }

  async getAllRecycleBinItems() {
    return await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.recycleBinElement}/root?skip=0&take=10000`);
  }

  async emptyRecycleBin() {
    return await this.api.delete(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.recycleBinElement}`);
  }

  async doesItemExistInRecycleBin(elementName: string) {
    const recycleBin = await this.getAllRecycleBinItems();
    const jsonRecycleBin = await recycleBin.json();
    for (const element of jsonRecycleBin.items) {
      if (element.name === elementName) {
        return true;
      }
    }
    return false;
  }

  // Folder
  async getFolder(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.elementFolder}/${id}`);
    return await response.json();
  }

  async deleteFolder(id: string) {
    return await this.api.delete(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.elementFolder}/${id}`);
  }

  async createFolder(name: string, parentId?: string) {
    const folder = {
      name: name,
      parent: parentId ? {id: parentId} : null

    }
    const response = await this.api.post(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.elementFolder}`, folder);
    return response.headers().location.split("/").pop();
  }

  async renameFolder(folderId: string, folderName: string) {
    const folder = {
      name: folderName
    }
    return await this.api.put(`${this.api.baseUrl}${ConstantHelper.apiEndpoints.elementFolder}/${folderId}`, folder);
  }

  // Create Elements
  async createDefaultElement(elementName: string, elementTypeId: string) {
    await this.ensureNameNotExists(elementName);

    const element = new ElementBuilder()
      .withDocumentTypeId(elementTypeId)
      .addVariant()
        .withName(elementName)
        .done()
      .build();

    return await this.create(element);
  }

  async createElementWithTextContent(elementName: string, elementTypeId: string, textContent: string, dataTypeName: string) {
    await this.ensureNameNotExists(elementName);

    const element = new ElementBuilder()
      .withDocumentTypeId(elementTypeId)
      .addVariant()
        .withName(elementName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .done()
      .build();

    return await this.create(element);
  }

  async createDefaultElementWithParent(elementName: string, elementTypeId: string, parentId: string) {
    await this.ensureNameNotExists(elementName);

    const element = new ElementBuilder()
      .withDocumentTypeId(elementTypeId)
      .withParentId(parentId)
      .addVariant()
        .withName(elementName)
        .done()
      .build();

    return await this.create(element);
  }

  async isElementPublished(id: string) {
    const element = await this.get(id);
    return element.variants[0].state === 'Published';
  }

  async createDefaultElementFolder(folderName: string, parentId?: string) {
    await this.ensureNameNotExists(folderName);

    return await this.createFolder(folderName, parentId);
  }

  async createElementWithElementPickers(elementName: string, elementTypeId: string, dataTypeName: string, elementPickerIds: string[]) {
    await this.ensureNameNotExists(elementName);

    const element = new ElementBuilder()
      .withDocumentTypeId(elementTypeId)
      .addVariant()
        .withName(elementName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(elementPickerIds)
        .done()
      .build();

    // Create element
    return await this.create(element);
  }

  async createDefaultElementWithEnglishAndDanishVariants(elementTypeId: string, englishName: string, danishName: string, propertyName: string, englishTextContent: string, danishTextContent: string) {
    await this.ensureNameNotExists(englishName);
    await this.ensureNameNotExists(danishName);

    const element = new ElementBuilder()
      .withDocumentTypeId(elementTypeId)
      .addVariant()
        .withName(englishName)
        .withCulture('en-US')
        .done()
      .addVariant()
        .withName(danishName)
        .withCulture('da')
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .withValue(englishTextContent)
        .withCulture('en-US')
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .withValue(danishTextContent)
        .withCulture('da')
        .done()
      .build();

    return await this.create(element);
  }
}