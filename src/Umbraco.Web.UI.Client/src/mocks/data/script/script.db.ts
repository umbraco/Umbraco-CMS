import { UmbData } from '../data.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { createTextFileItem } from '../utils.js';
import { UmbMockScriptModel, data as scriptData } from './script.data.js';
import {
	CreateTextFileViewModelBaseModel,
	ScriptResponseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbScriptMockDB extends UmbData<UmbMockScriptModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockScriptModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockScriptModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockScriptModel>(this);

	constructor(data: Array<UmbMockScriptModel>) {
		super(data);
	}

	create(item: CreateTextFileViewModelBaseModel) {
		const newItem: UmbMockScriptModel = {
			name: item.name,
			content: item.content,
			parentPath: item.parentPath,
			path: `${item.parentPath}` ? `${item.parentPath}/${item.name}}` : item.name,
			isFolder: false,
			hasChildren: false,
			type: 'script',
		};

		this.data.push(newItem);
	}

	read(path: string): ScriptResponseModel | undefined {
		const item = this.data.find((item) => item.path === path);
		return createTextFileItem(item);
	}

	update(updateItem: UpdateScriptRequestModel) {
		const itemIndex = this.data.findIndex((item) => item.path === updateItem.existingPath);
		const item = this.data[itemIndex];
		if (!item) throw new Error('Item not found');

		const updatedItem = {
			path: item,
		};

		this.data[itemIndex] = newItem;
	}

	delete(paths: Array<string>) {
		this.data = this.data.filter((item) => {
			if (!item.path) throw new Error('Item has no path');
			return paths.indexOf(item.path) === -1;
		});
	}
}

export const umbScriptMockDb = new UmbScriptMockDB(scriptData);
