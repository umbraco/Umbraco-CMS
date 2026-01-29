import type { UmbMockPartialViewModel } from '../data/sets/index.js';
import { dataSet } from '../data/sets/index.js';
import { UmbFileSystemMockDbBase } from './utils/file-system/file-system-base.js';
import { UmbMockFileSystemFolderManager } from './utils/file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from './utils/file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from './utils/file-system/file-system-tree.manager.js';
import { UmbMockFileSystemDetailManager } from './utils/file-system/file-system-detail.manager.js';
import type {
	PagedPartialViewSnippetItemResponseModel,
	PartialViewSnippetItemResponseModel,
	PartialViewSnippetResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbPartialViewMockDB extends UmbFileSystemMockDbBase<UmbMockPartialViewModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockPartialViewModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockPartialViewModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockPartialViewModel>(this);
	file = new UmbMockFileSystemDetailManager<UmbMockPartialViewModel>(this);

	constructor(data: Array<UmbMockPartialViewModel>) {
		super(data);
	}

	getSnippets(): PagedPartialViewSnippetItemResponseModel {
		const snippetItems = (dataSet.partialViewSnippets ?? []).map((item) => createSnippetItem(item));
		const total = snippetItems.length;
		return { items: snippetItems, total };
	}

	getSnippet(id: string): PartialViewSnippetResponseModel | undefined {
		return (dataSet.partialViewSnippets ?? []).find((item) => item.id === id);
	}
}

const createSnippetItem = (item: PartialViewSnippetResponseModel): PartialViewSnippetItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
	};
};

export const umbPartialViewMockDB = new UmbPartialViewMockDB(dataSet.partialView ?? []);
