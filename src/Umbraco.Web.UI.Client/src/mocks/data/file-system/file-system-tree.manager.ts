import type { UmbData } from '../data.js';
import { createFileSystemTreeItem } from '../utils.js';
import type {
	FileSystemTreeItemPresentationModel} from '@umbraco-cms/backoffice/backend-api';
import {
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemTreeManager<T extends Omit<FileSystemTreeItemPresentationModel, 'type'>> {
	#db: UmbData<T>;

	constructor(mockDb: UmbData<T>) {
		this.#db = mockDb;
	}

	getRoot(): { items: Array<Omit<FileSystemTreeItemPresentationModel, 'type'>>; total: number } {
		const items = this.#db.getData().filter((item) => item.parent === null);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getChildrenOf(parentPath: string): {
		items: Array<Omit<FileSystemTreeItemPresentationModel, 'type'>>;
		total: number;
	} {
		const items = this.#db.getData().filter((item) => item.parent?.path === parentPath);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}
}
