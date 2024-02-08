import { UmbFileSystemMockDbBase } from '../utils/file-system/file-system-base.js';
import { UmbMockFileSystemFolderManager } from '../utils/file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../utils/file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../utils/file-system/file-system-tree.manager.js';
import { UmbMockFileSystemDetailManager } from '../utils/file-system/file-system-detail.manager.js';
import type { UmbMockScriptModel } from './script.data.js';
import { data as scriptData } from './script.data.js';

class UmbScriptMockDB extends UmbFileSystemMockDbBase<UmbMockScriptModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockScriptModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockScriptModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockScriptModel>(this);
	file = new UmbMockFileSystemDetailManager<UmbMockScriptModel>(this);

	constructor(data: Array<UmbMockScriptModel>) {
		super(data);
	}
}

export const umbScriptMockDb = new UmbScriptMockDB(scriptData);
