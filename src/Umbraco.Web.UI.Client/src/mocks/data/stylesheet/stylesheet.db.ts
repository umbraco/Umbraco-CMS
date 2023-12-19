import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { textFileItemMapper } from '../utils.js';
import { UmbMockStylesheetModel, data } from './stylesheet.data.js';
import {
	CreateStylesheetRequestModel,
	PagedStylesheetOverviewResponseModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbStylesheetData extends UmbFileSystemMockDbBase<UmbMockStylesheetModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStylesheetModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStylesheetModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockStylesheetModel>(this);
	file;

	constructor(data: Array<UmbMockStylesheetModel>) {
		super(data);

		this.file = new UmbMockFileSystemDetailManager<UmbMockStylesheetModel>(this, {
			createMapper: this.#createStylesheetMockItemMapper,
			readMapper: this.#readStylesheetResponseMapper,
		});
	}

	getAllStylesheets(): PagedStylesheetOverviewResponseModel {
		return {
			items: this.data.map((item) => textFileItemMapper(item)),
			total: this.data.map((item) => !item.isFolder).length,
		};
	}

	#createStylesheetMockItemMapper = (item: CreateStylesheetRequestModel): UmbMockStylesheetModel => {
		return {
			name: item.name,
			content: item.content,
			path: `${item.parentPath}` ? `${item.parentPath}/${item.name}` : item.name,
			isFolder: false,
			hasChildren: false,
			type: 'stylesheet',
		};
	};

	#readStylesheetResponseMapper = (item: UmbMockStylesheetModel): StylesheetResponseModel => {
		return {
			path: item.path,
			name: item.name,
			content: item.content,
		};
	};
}

export const umbStylesheetData = new UmbStylesheetData(data);
