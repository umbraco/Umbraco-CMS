import type { UmbMockStylesheetModel } from '../data/sets/index.js';
import { dataSet } from '../data/sets/index.js';
import { UmbFileSystemMockDbBase } from './utils/file-system/file-system-base.js';
import { UmbMockFileSystemDetailManager } from './utils/file-system/file-system-detail.manager.js';
import { UmbMockFileSystemFolderManager } from './utils/file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from './utils/file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from './utils/file-system/file-system-tree.manager.js';

class UmbStylesheetMockDb extends UmbFileSystemMockDbBase<UmbMockStylesheetModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStylesheetModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStylesheetModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockStylesheetModel>(this);
	file = new UmbMockFileSystemDetailManager<UmbMockStylesheetModel>(this);

	constructor(data: Array<UmbMockStylesheetModel>) {
		super(data);
	}
}

export const umbStylesheetMockDb = new UmbStylesheetMockDb(dataSet.stylesheet ?? []);
