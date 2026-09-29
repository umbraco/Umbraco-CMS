import { pagedResult } from '../paged-result.js';
import { UmbId } from '@umbraco-cms/backoffice/id';

/**
 * Interface for DB classes that can be used with the tree manager.
 * Both UmbEntityMockDbBase and UmbEntityRecycleBin implement this.
 */
export interface UmbMockEntityTreeSource<T> {
	getAll(): Array<T>;
	read(id: string): T | undefined;
	update?(id: string, item: T): void;
	create?(item: T): string | void;
}

export class UmbMockEntityTreeManager<T extends { id: string; parent?: { id: string } | null; hasChildren: boolean }> {
	#db: UmbMockEntityTreeSource<T>;
	#treeItemMapper: (item: T) => any;

	constructor(mockDb: UmbMockEntityTreeSource<T>, treeItemMapper: (item: T) => any) {
		this.#db = mockDb;
		this.#treeItemMapper = treeItemMapper;
	}

	/** Excludes trashed items by default (no-op if there's no `isTrashed`). Overridden to show only trashed items. */
	protected _isItemVisible(item: T): boolean {
		return !(item as { isTrashed?: boolean }).isTrashed;
	}

	protected _getVisibleItems(): Array<T> {
		return this.#db.getAll().filter((item) => this._isItemVisible(item));
	}

	getRoot({ skip = 0, take = 100 }: { skip?: number; take?: number } = {}) {
		const items = this._getVisibleItems().filter((item) => item.parent === null || item.parent === undefined);
		return this.#pagedTreeResult({ items, skip, take });
	}

	getChildrenOf({ parentId, skip = 0, take = 100 }: { parentId: string; skip?: number; take?: number }) {
		const items = this._getVisibleItems().filter((item) => item.parent?.id === parentId);
		return this.#pagedTreeResult({ items, skip, take });
	}

	getAncestorsOf({ descendantId }: { descendantId: string }): Array<T> {
		const items = [];
		let currentId: string | undefined = descendantId;
		while (currentId) {
			const item = this.#db.read(currentId);
			if (!item) break;
			items.push(item);
			currentId = item.parent?.id;
		}
		return items.reverse();
	}

	#pagedTreeResult({ items, skip, take }: { items: Array<T>; skip: number; take: number }) {
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => this.#treeItemMapper(item));
		const treeItemsHasChildren = treeItems.map((item) => {
			const children = this._getVisibleItems().filter((child) => child.parent?.id === item.id);
			return {
				...item,
				hasChildren: children.length > 0,
			};
		});
		return { items: treeItemsHasChildren, total: paged.total };
	}

	/** Returns `null` (not a hollow empty result) when `targetId` isn't resolvable in this tree, so callers can respond with a proper "not found" instead of masking the failure. */
	getSiblingsOf({
		targetId,
		before = 0,
		after = 100,
	}: {
		targetId: string;
		before?: number;
		after?: number;
	}): { items: Array<unknown>; totalBefore: number; totalAfter: number } | null {
		const target = this.#db.read(targetId);
		if (!target || !this._isItemVisible(target)) return null;

		const parentId = target.parent?.id ?? null;
		const allSiblings = this._getVisibleItems().filter((item) =>
			parentId === null ? item.parent === null || item.parent === undefined : item.parent?.id === parentId,
		);

		const targetIndex = allSiblings.findIndex((item) => item.id === targetId);
		if (targetIndex === -1) return null;

		const startIndex = Math.max(0, targetIndex - before);
		const endIndex = Math.min(allSiblings.length, targetIndex + after + 1);
		const slicedItems = allSiblings.slice(startIndex, endIndex);

		// totalBefore/totalAfter represent items outside the returned window, so the client knows if there are more items to paginate to.
		const totalBefore = startIndex;
		const totalAfter = allSiblings.length - endIndex;

		const treeItems = slicedItems.map((item) => this.#treeItemMapper(item));
		const treeItemsHasChildren = treeItems.map((item) => {
			const children = this._getVisibleItems().filter((child) => child.parent?.id === item.id);
			return { ...item, hasChildren: children.length > 0 };
		});

		return { items: treeItemsHasChildren, totalBefore, totalAfter };
	}

	/** A `null`/`undefined` `destinationId` moves the items to the tree root. */
	move(ids: Array<string>, destinationId: string | null | undefined) {
		if (!this.#db.update) throw new Error('move() requires a DB with update() method');

		const destinationItem = destinationId ? this.#db.read(destinationId) : undefined;
		if (destinationId && !destinationItem) throw new Error(`Destination item with id ${destinationId} not found`);

		const items: Array<any> = [];

		ids.forEach((id) => {
			const item = this.#db.read(id);
			if (!item) throw new Error(`Item with id ${id} not found`);
			items.push(item);
		});

		const movedItems = items.map((item) => {
			return {
				...item,
				parent: destinationId ? { id: destinationId } : null,
			};
		});

		movedItems.forEach((movedItem: any) => this.#db.update!(movedItem.id, movedItem));

		if (destinationItem) {
			destinationItem.hasChildren = true;
			this.#db.update(destinationItem.id, destinationItem);
		}
	}

	/** A `null`/`undefined` `destinationId` copies the items to the tree root. */
	copy(ids: Array<string>, destinationId: string | null | undefined) {
		if (!this.#db.update || !this.#db.create)
			throw new Error('copy() requires a DB with update() and create() methods');

		const destinationItem = destinationId ? this.#db.read(destinationId) : undefined;
		if (destinationId && !destinationItem) throw new Error(`Destination item with id ${destinationId} not found`);

		// Notice we don't add numbers to the 'copy' name.
		const items: Array<any> = [];

		ids.forEach((id) => {
			const item = this.#db.read(id);
			if (!item) throw new Error(`Item with id ${id} not found`);
			items.push(item);
		});

		const copyItems = items.map((item) => {
			return {
				...item,
				name: item.name + ' Copy',
				id: UmbId.new(),
				parent: destinationId ? { id: destinationId } : null,
			};
		});

		copyItems.forEach((copyItem) => this.#db.create!(copyItem));
		const newIds = copyItems.map((item) => item.id);

		if (destinationItem) {
			destinationItem.hasChildren = true;
			this.#db.update(destinationItem.id, destinationItem);
		}

		return newIds;
	}
}
