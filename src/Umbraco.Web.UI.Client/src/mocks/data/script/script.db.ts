import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockScriptModel, data as scriptData } from './script.data.js';
import { CreateScriptRequestModel, ScriptResponseModel } from '@umbraco-cms/backoffice/backend-api';

class UmbScriptMockDB extends UmbFileSystemMockDbBase<UmbMockScriptModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockScriptModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockScriptModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockScriptModel>(this);
	file;

	constructor(data: Array<UmbMockScriptModel>) {
		super(data);

		this.file = new UmbMockFileSystemDetailManager<UmbMockScriptModel>(this, {
			createMapper: this.#createScriptMockItem,
			readMapper: this.#readScriptResponseMapper,
		});
	}

	#createScriptMockItem = (item: CreateScriptRequestModel, path: string): UmbMockScriptModel => {
		return {
			name: item.name,
			content: item.content,
			path: path,
			parentPath: item.parentPath || null,
			isFolder: false,
			hasChildren: false,
		};
	};

	#readScriptResponseMapper = (item: UmbMockScriptModel): ScriptResponseModel => {
		return {
			name: item.name,
			path: item.path,
			parentPath: item.parentPath,
			content: item.content,
		};
	};
}

export const umbScriptMockDb = new UmbScriptMockDB(scriptData);
