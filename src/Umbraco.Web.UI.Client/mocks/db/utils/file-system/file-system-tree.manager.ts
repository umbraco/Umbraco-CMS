import type { UmbMockDBBase } from '../mock-db-base.js';
import { pagedResult } from '../paged-result.js';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockFileSystemTreeManager<T extends FileSystemTreeItemPresentationModel> {
	#db: UmbMockDBBase<T>;

	constructor(mockDb: UmbMockDBBase<T>) {
		this.#db = mockDb;
	}

	getRoot({ skip = 0, take = 100 }: { skip?: number; take?: number } = {}): {
		items: Array<FileSystemTreeItemPresentationModel>;
		total: number;
	} {
		const items = this.#db.getAll().filter((item) => item.parent === null);
		return this.#pagedTreeResult({ items, skip, take });
	}

	getChildrenOf({ parentPath, skip = 0, take = 100 }: { parentPath: string; skip?: number; take?: number }): {
		items: Array<FileSystemTreeItemPresentationModel>;
		total: number;
	} {
		const items = this.#db.getAll().filter((item) => item.parent?.path === parentPath);
		return this.#pagedTreeResult({ items, skip, take });
	}

	getSiblingsOf({ path, before = 0, after = 100 }: { path: string; before?: number; after?: number }) {
		const target = this.#db.getAll().find((item) => item.path === path);
		if (!target) return { items: [], totalBefore: 0, totalAfter: 0 };

		const parentPath = target.parent?.path ?? null;
		const allSiblings = this.#db.getAll().filter((item) =>
			parentPath === null ? item.parent === null : item.parent?.path === parentPath,
		);

		const targetIndex = allSiblings.findIndex((item) => item.path === path);
		if (targetIndex === -1) return { items: [], totalBefore: 0, totalAfter: 0 };

		const startIndex = Math.max(0, targetIndex - before);
		const endIndex = Math.min(allSiblings.length, targetIndex + after + 1);
		const slicedItems = allSiblings.slice(startIndex, endIndex);

		// totalBefore/totalAfter represent items outside the returned window, so the client knows if there are more items to paginate to.
		const totalBefore = startIndex;
		const totalAfter = allSiblings.length - endIndex;

		const treeItems = slicedItems.map((item) => createFileSystemTreeItem(item));
		const treeItemsHasChildren = treeItems.map((item) => {
			const children = this.#db.getAll().filter((child) => child.parent?.path === item.path);
			return { ...item, hasChildren: children.length > 0 };
		});

		return { items: treeItemsHasChildren, totalBefore, totalAfter };
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

export const createFileSystemTreeItem = (item: any): FileSystemTreeItemPresentationModel => {
	return {
		path: item.path,
		parent: item.parent ?? null,
		name: item.name,
		hasChildren: item.hasChildren ?? false,
		isFolder: item.isFolder ?? false,
	};
};
