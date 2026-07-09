import {ApiHelpers} from "./ApiHelpers";

export class ScriptApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/script/' + encodeURIComponent(path));
    return await response.json();
  }

  async doesExist(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/script/' + encodeURIComponent(path));
    return response.status() === 200;
  }

  async create(name: string, content: string, parentPath = null) {
    const parentValue = parentPath ? {'path': parentPath} : null;
    const scriptData = {
      "name": name,
      "parent": parentValue,
      "content": content
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/script', scriptData);
    // Returns the path of the created Script
    const path = response.headers().location.split("/v1/script/").pop() as string;
    return decodeURIComponent(path);
  }

  async updateName(path: string, newName: string) {
    const scriptData = {
      "name": newName
    };
    const response = await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/script/' + encodeURIComponent(path) + '/rename', scriptData);
    // Returns the path of the created Script
    const newPath = response.headers().location.split("/v1/script/").pop();
    if (newPath !== undefined) {
      return decodeURIComponent(newPath);
    }
    return undefined;
  }

  async updateContent(path: string, newContent: string) {
    const scriptData = {
      "content": newContent
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/script/' + encodeURIComponent(path), scriptData);
  }

  async delete(path: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/script/' + encodeURIComponent(path));
  }

  async getChildren(path: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/script/children?parentPath=${encodeURIComponent(path)}&skip=0&take=10000`);
    const items = await response.json();
    return items.items;
  }

  async getItems(paths: string[]) {
    let pathArray = 'path=' + paths[0];
    let i: number;

    for (i = 1; i < paths.length; ++i) {
      pathArray += '&path=' + paths[i];
    }

    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/script/item?' + pathArray);
    const json = await response.json();
    if (json !== null) {
      return json;
    }
    return null;
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/script/root?skip=0&take=10000');
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getByName(name: string) {
    const rootScripts = await this.getAllAtRoot();
    const jsonScripts = await rootScripts.json();

    for (const script of jsonScripts.items) {
      if (script.name === name) {
        if (script.isFolder) {
          return this.getFolder(script.path);
        } else {
          return this.get(script.path);
        }
      } else if (script.isFolder && script.hasChildren) {
        const result = await this.recurseChildren(name, script.path, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  private async recurseDeleteChildren(scriptFolder) {
    if (!scriptFolder.hasChildren) {
      return await this.deleteFolder(scriptFolder.path);
    }
    const items = await this.getChildren(scriptFolder.path);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else if (child.isFolder) {
        await this.deleteFolder(child.path);
      } else {
        await this.delete(child.path);
      }
    }
    return await this.deleteFolder(scriptFolder.path);
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

  async ensureNameNotExists(name: string) {
    const rootScripts = await this.getAllAtRoot();
    const jsonScripts = await rootScripts.json();

    for (const script of jsonScripts.items) {
      if (script.name === name) {
        if (script.isFolder) {
          return await this.recurseDeleteChildren(script);
        }
        return await this.delete(script.path);
      } else if (script.hasChildren) {
        await this.recurseChildren(name, script.path, true);

      }
    }
    return null;
  }

  // Folder
  async getFolder(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/script/folder/' + encodeURIComponent(path));
    return await response.json();
  }

  async doesFolderExist(path: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/script/folder/' + encodeURIComponent(path));
    return response.status() === 200;
  }

  async createFolder(name: string, parentPath?: string) {
    const parentValue = parentPath ? {'path': parentPath} : null;
    const scriptFolderData =
      {
        "name": name,
        "parent": parentValue
      };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/script/folder', scriptFolderData);
    // Returns the path of the created Stylesheet folder
    const path = response.headers().location.split("/v1/script/folder/").pop();

    if (path !== undefined) {
      return decodeURIComponent(path);
    }
    return undefined;
  }

  async deleteFolder(path: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/script/folder/' + encodeURIComponent(path));
  }

  async createDefaultScript(name: string) {
    await this.ensureNameNotExists(name);
    return await this.create(name, "@inherits Umbraco.Web.Mvc.UmbracoViewPage");
  }
}