import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { textFileItemMapper } from '../utils.js';
import { UmbMockStylesheetModel, data } from './stylesheet.data.js';
import { PagedStylesheetOverviewResponseModel } from '@umbraco-cms/backoffice/backend-api';

class UmbStylesheetData extends UmbFileSystemMockDbBase<UmbMockStylesheetModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStylesheetModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStylesheetModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockStylesheetModel>(this);
	file = new UmbMockFileSystemDetailManager<UmbMockStylesheetModel>(this);

	constructor(data: Array<UmbMockStylesheetModel>) {
		super(data);
	}

	getAllStylesheets(): PagedStylesheetOverviewResponseModel {
		return {
			items: this.data.map((item) => textFileItemMapper(item)),
			total: this.data.map((item) => !item.isFolder).length,
		};
	}
}

export const umbStylesheetData = new UmbStylesheetData(data);
