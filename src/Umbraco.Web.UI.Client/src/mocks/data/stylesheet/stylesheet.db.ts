import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { textFileItemMapper } from '../utils.js';
import { UmbMockStylesheetModel, data } from './stylesheet.data.js';
import { PagedStylesheetOverviewResponseModel } from '@umbraco-cms/backoffice/backend-api';

interface UmbMockPaginationModel {
	skip?: number;
	take?: number;
}

class UmbStylesheetMockDb extends UmbFileSystemMockDbBase<UmbMockStylesheetModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStylesheetModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStylesheetModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockStylesheetModel>(this);
	file = new UmbMockFileSystemDetailManager<UmbMockStylesheetModel>(this);

	constructor(data: Array<UmbMockStylesheetModel>) {
		super(data);
	}

	getOverview(filterOptions: UmbMockPaginationModel = { skip: 0, take: 100 }): PagedStylesheetOverviewResponseModel {
		const mockItems = this.getData();
		const files = mockItems.filter((item) => item.isFolder === false);
		const paginatedFiles = files.slice(filterOptions.skip, filterOptions.skip! + filterOptions.take!);
		const responseItems = paginatedFiles.map((item) => {
			return {
				name: item.name,
				path: item.path,
			};
		});

		return { items: responseItems, total: mockItems.length };
	}
}

export const umbStylesheetMockDb = new UmbStylesheetMockDb(data);
