import {ApiHelpers} from "./ApiHelpers";

export class PartialViewApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/' + encodeURIComponent(path));
    return await response.json();
  }

  async doesExist(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/' + encodeURIComponent(path));
    return response.status() === 200;
  }

  async create(name: string, content: string, parentPath?: string) {
    const parentValue = parentPath ? {'path': parentPath} : null;
    const partialViewData = {
      'name': name,
      'parent': parentValue,
      'content': content
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/partial-view', partialViewData);
    // Returns the path of the created partialView
    const partialViewPath = response.headers().location.split("/").pop() as string;
    return decodeURIComponent(partialViewPath);
  }

  async updateContent(path: string, content: string) {
    const contentData = {
      'content': content
    }
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/'+ encodeURIComponent(path), contentData);
  }

  async updateName(path: string, name: string) {
    const renameData = {
      'name': name
    }
    const response = await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/'+ encodeURIComponent(path) + '/rename', renameData);
    // Returns the path of the renamed partialView
    const partialViewPath = response.headers().location.split("/").pop() as string;
    return decodeURIComponent(partialViewPath);
  }

  async delete(path: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/' + encodeURIComponent(path));
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getChildren(path: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/partial-view/children?parentPath=${path}&skip=0&take=10000`);
    const items = await response.json();
    return items.items;
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/partial-view/root?skip=0&take=10000');
  }

  async getByName(name: string) {
    const rootPartialView = await this.getAllAtRoot();
    const jsonPartialView = await rootPartialView.json();

    for (const partialView of jsonPartialView.items) {
      if (partialView.name === name) {
        if (partialView.isFolder) {
          return this.getFolder(partialView.path);
        } else {
          return this.get(partialView.path);
        }
      } else if (partialView.isFolder && partialView.hasChildren) {
        const result = await this.recurseChildren(name, partialView.path, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootPartialView = await this.getAllAtRoot();
    const jsonPartialView = await rootPartialView.json();

    for (const partialView of jsonPartialView.items) {
      if (partialView.name === name) {
        if (partialView.isFolder) {
          return await this.recurseDeleteChildren(partialView);
        }
        return await this.delete(partialView.path);
      } else if (partialView.hasChildren) {
        await this.recurseChildren(name, partialView.path, true);
      }
    }
    return null;
  }

  private async recurseChildren(name: string, path: string, toDelete: boolean) {
    const items = await this.getChildren(path);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          if (child.isFolder) {
            return await this.getFolder(child.path);
          }
          return await this.get(child.path);
        }
        if (child.isFolder) {
          return await this.recurseDeleteChildren(child);
        }
        return await this.delete(child.path);
      } else if (child.hasChildren) {
        return await this.recurseChildren(name, child.path, toDelete);
      }
    }
    return false;
  }

  private async recurseDeleteChildren(partialViewFolder) {
    if (!partialViewFolder.hasChildren) {
      return await this.deleteFolder(partialViewFolder.path);
    }
    const items = await this.getChildren(partialViewFolder.path);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else if (child.isFolder) {
        await this.deleteFolder(child.path);
      } else {
        await this.delete(child.path);
      }
    }
    return await this.deleteFolder(partialViewFolder.path);
  }

  // Folder
  async getFolder(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/folder/' + encodeURIComponent(path));
    return await response.json();
  }

  async doesFolderExist(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/folder/' + encodeURIComponent(path));
    return response.status() === 200;
  }

  async createFolder(name: string, parentPath?: string) {
    const parentValue = parentPath ? {'path': parentPath} : null;
    const partialViewFolderData =
      {
        "name": name,
        "parent": parentValue
      };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/folder', partialViewFolderData);
    // Returns the path of the created folder
    const folderPath = response.headers().location.split("/").pop() as string;
    return decodeURIComponent(folderPath);
  }

  async deleteFolder(path: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/partial-view/folder/' + encodeURIComponent(path));
  }

  async createDefaultPartialView(name: string) {
    await this.ensureNameNotExists(name);
    return await this.create(name, "<h1>Test</h1>");
  }
}