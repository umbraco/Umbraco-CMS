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
		return this.#pagedTreeResult({ items, skip, take });
	}

	getChildrenOf({ parentPath, skip = 0, take = 100 }: { parentPath: string; skip?: number; take?: number }): {
		items: Array<Omit<FileSystemTreeItemPresentationModel, 'type'>>;
		total: number;
	} {
		const items = this.#db.getAll().filter((item) => item.parent?.path === parentPath);
		return this.#pagedTreeResult({ items, skip, take });
	}

	#pagedTreeResult({ items, skip, take }: { items: Array<T>; skip: number; take: number }) {
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => createFileSystemTreeItem(item));
		const treeItemsHasChildren = treeItems.map((item) => {
			const children = this.#db.getAll().filter((child) => child.parent?.path === item.path);
			return {
				...item,
				hasChildren: children.length > 0,
			};
		});
		return { items: treeItemsHasChildren, total: paged.total };
	}
}
