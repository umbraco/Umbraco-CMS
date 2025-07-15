import { UmbFileSystemMockDbBase } from '../utils/file-system/file-system-base.js';
import { UmbMockFileSystemItemManager } from '../utils/file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../utils/file-system/file-system-tree.manager.js';
import type { UmbMockStaticFileModel } from './static-file.data.js';
import { data as staticFileData } from './static-file.data.js';

class UmbStaticFileMockDB extends UmbFileSystemMockDbBase<UmbMockStaticFileModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStaticFileModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStaticFileModel>(this);

	constructor(data: Array<UmbMockStaticFileModel>) {
		super(data);
	}
}

export const umbStaticFileMockDb = new UmbStaticFileMockDB(staticFileData);
