import {ApiHelpers} from "./ApiHelpers";

export class DictionaryApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/' + id);
    return await response.json();
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/' + id);
    return response.status() === 200;
  }

  async create(name: string, translations?: { isoCode: string, translation: string }[], parentId?: string) {
    const dictionary = {
      "name": name,
      "translations": translations,
      "parent": parentId ? {"id" : parentId} : null
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/dictionary', dictionary);
    // Returns the id of the created dictionary
    return response.headers().location.split("/").pop();
  }

  async update(id: string, dictionary: object) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/' + id, dictionary);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/' + id);
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/dictionary/root?skip=0&take=10000');
  }

  async getItems(ids: string[]) {
    let idArray = 'id=' + ids[0];
    let i: number;

    for (i = 1; i < ids.length; ++i) {
      idArray += '&id=' + ids[i];
    }
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/dictionary/item?' + idArray);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getChildren(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/dictionary/children?parentId=' + id + '&skip=0&take=10000');
    const items = await response.json();
    return items.items;
  }

  private async recurseDeleteChildren(id: string) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.isContainer || child.hasChildren) {
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
        if (child.isContainer || child.hasChildren) {
          return await this.recurseDeleteChildren(child.id);
        } else {
          return await this.delete(child.id);
        }
      } else if (child.isContainer || child.hasChildren) {
        await this.recurseChildren(name, child.id, toDelete);
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootDictionary = await this.getAllAtRoot();
    const jsonDictionary = await rootDictionary.json();

    for (const dictionaryItem of jsonDictionary.items) {
      if (dictionaryItem.name === name) {
        return this.get(dictionaryItem.id);
      } else if (dictionaryItem.isContainer || dictionaryItem.hasChildren) {
        const result = await this.recurseChildren(name, dictionaryItem.id, false);
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

  async ensureNameNotExists(name: string) {
    const rootDictionary = await this.getAllAtRoot();
    const jsonDictionary = await rootDictionary.json();

    for (const dictionaryItem of jsonDictionary.items) {
      if (dictionaryItem.name === name) {
        if (dictionaryItem.isContainer || dictionaryItem.hasChildren) {
          await this.recurseDeleteChildren(dictionaryItem.id);
        }
        await this.delete(dictionaryItem.id);
      } else {
        if (dictionaryItem.isContainer || dictionaryItem.hasChildren) {
          await this.recurseChildren(name, dictionaryItem.id, true);
        }
      }
    }
  }

  async export(id: string, includeChildren: boolean) {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/' + id + '/export?includeChildren=' + includeChildren);
  }

  async import(temporaryFileId: string, parentId: string) {
    const importDictionary = {
      "temporaryFile": temporaryFileId ? {"id" : temporaryFileId} : null,
      "parent": parentId ? {"id" : parentId} : null
    }
    return await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/dictionary/import', importDictionary);
  }

  async createDefaultDictionary(name: string) {
    await this.ensureNameNotExists(name);
    const allLanguages = await this.api.language.getAll();
    const jsonLanguages = await allLanguages.json();
    const languageIsoCode = jsonLanguages.items[0].isoCode;
    const translations = [
      {
        "isoCode": languageIsoCode,
        "translation": name
      }
    ];

    return await this.create(name, translations);
  }
}