import { UmbData } from '../data.js';
import { createFileSystemTreeItem } from '../utils.js';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemTreeManager<T extends FileSystemTreeItemPresentationModel> {
	#db: UmbData<T>;

	constructor(mockDb: UmbData<T>) {
		this.#db = mockDb;
	}

	getTreeRoot(): PagedFileSystemTreeItemPresentationModel {
		const items = this.#db.getData().filter((item) => item.path?.includes('/') === false);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(parentPath: string): PagedFileSystemTreeItemPresentationModel {
		const items = this.#db.getData().filter((item) => item.path?.startsWith(parentPath));
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(path: string): FileSystemTreeItemPresentationModel | undefined {
		return this.#db.getData().find((item) => item.path === path);
	}
}
