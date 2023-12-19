import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockPartialViewModel, data } from './partial-view.data.js';
import { CreatePartialViewRequestModel, PartialViewResponseModel } from '@umbraco-cms/backoffice/backend-api';

class UmbPartialViewMockDB extends UmbFileSystemMockDbBase<UmbMockPartialViewModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockPartialViewModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockPartialViewModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockPartialViewModel>(this);
	file;

	constructor(data: Array<UmbMockPartialViewModel>) {
		super(data);

		this.file = new UmbMockFileSystemDetailManager<UmbMockPartialViewModel>(this, {
			createMapper: this.#createPartialViewMockItemMapper,
			readMapper: this.#readPartialResponseMapper,
		});
	}

	#createPartialViewMockItemMapper = (item: CreatePartialViewRequestModel): UmbMockPartialViewModel => {
		return {
			name: item.name,
			content: item.content,
			path: item.path,
			parentPath: item.parentPath,
			isFolder: false,
			hasChildren: false,
		};
	};

	#readPartialResponseMapper = (item: UmbMockPartialViewModel): PartialViewResponseModel => {
		return {
			path: item.path,
			name: item.name,
			content: item.content,
		};
	};
}

export const umbPartialViewMockDB = new UmbPartialViewMockDB(data);
