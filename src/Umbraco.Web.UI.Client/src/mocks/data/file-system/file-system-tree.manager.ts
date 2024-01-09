import { UmbData } from '../data.js';
import { createFileSystemTreeItem } from '../utils.js';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemTreeManager<T extends Omit<FileSystemTreeItemPresentationModel, 'type' | 'icon'>> {
	#db: UmbData<T>;

	constructor(mockDb: UmbData<T>) {
		this.#db = mockDb;
	}

	getRoot(): PagedFileSystemTreeItemPresentationModel {
		const items = this.#db.getData().filter((item) => item.parent === null);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getChildrenOf(parentPath: string): PagedFileSystemTreeItemPresentationModel {
		const items = this.#db.getData().filter((item) => item.parent?.path === parentPath);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}
}
