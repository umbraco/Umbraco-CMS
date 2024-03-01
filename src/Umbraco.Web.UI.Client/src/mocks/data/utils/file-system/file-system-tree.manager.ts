import type { UmbMockDBBase } from '../mock-db-base.js';
import { createFileSystemTreeItem } from '../../utils.js';
import { pagedResult } from '../paged-result.js';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockFileSystemTreeManager<T extends Omit<FileSystemTreeItemPresentationModel, 'type'>> {
	#db: UmbMockDBBase<T>;

	constructor(mockDb: UmbMockDBBase<T>) {
		this.#db = mockDb;
	}

	getRoot({ skip = 0, take = 100 }: { skip?: number; take?: number } = {}): {
		items: Array<Omit<FileSystemTreeItemPresentationModel, 'type'>>;
		total: number;
	} {
		const items = this.#db.getAll().filter((item) => item.parent === null);
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => createFileSystemTreeItem(item));
		return { items: treeItems, total: paged.total };
	}

	getChildrenOf({ parentPath, skip = 0, take = 100 }: { parentPath: string; skip?: number; take?: number }): {
		items: Array<Omit<FileSystemTreeItemPresentationModel, 'type'>>;
		total: number;
	} {
		const items = this.#db.getAll().filter((item) => item.parent?.path === parentPath);
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => createFileSystemTreeItem(item));
		return { items: treeItems, total: paged.total };
	}
}
