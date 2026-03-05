import type { UmbMockPartialViewModel } from '../data/types/mock-data-set.types.js';
import { umbMockManager } from '../mock-manager.js';
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
		super('partialView', data);
	}

	getSnippets(): PagedPartialViewSnippetItemResponseModel {
		const snippets = umbMockManager.getDataSet().partialViewSnippets ?? [];
		const snippetItems = snippets.map((item) => createSnippetItem(item));
		const total = snippetItems.length;
		return { items: snippetItems, total };
	}

	getSnippet(id: string): PartialViewSnippetResponseModel | undefined {
		const snippets = umbMockManager.getDataSet().partialViewSnippets ?? [];
		return snippets.find((item) => item.id === id);
	}
}

const createSnippetItem = (item: PartialViewSnippetResponseModel): PartialViewSnippetItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
	};
};

export const umbPartialViewMockDB = new UmbPartialViewMockDB([]);
