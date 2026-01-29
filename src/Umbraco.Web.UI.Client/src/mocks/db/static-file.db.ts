import type { UmbMockStaticFileModel } from '../data/sets/index.js';
import { dataSet } from '../data/sets/index.js';
import { UmbFileSystemMockDbBase } from './utils/file-system/file-system-base.js';
import { UmbMockFileSystemItemManager } from './utils/file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from './utils/file-system/file-system-tree.manager.js';

class UmbStaticFileMockDB extends UmbFileSystemMockDbBase<UmbMockStaticFileModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStaticFileModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStaticFileModel>(this);

	constructor(data: Array<UmbMockStaticFileModel>) {
		super(data);
	}
}

export const umbStaticFileMockDb = new UmbStaticFileMockDB(dataSet.staticFile ?? []);
