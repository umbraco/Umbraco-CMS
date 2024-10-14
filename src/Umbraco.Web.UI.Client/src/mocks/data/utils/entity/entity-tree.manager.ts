import { pagedResult } from '../paged-result.js';
import type { UmbEntityMockDbBase } from './entity-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbMockEntityTreeManager<T extends { id: string; parent?: { id: string } | null; hasChildren: boolean }> {
	#db: UmbEntityMockDbBase<T>;
	#treeItemMapper: (item: T) => any;

	constructor(mockDb: UmbEntityMockDbBase<T>, treeItemMapper: (item: T) => any) {
		this.#db = mockDb;
		this.#treeItemMapper = treeItemMapper;
	}

	getRoot({ skip = 0, take = 100 }: { skip?: number; take?: number } = {}) {
		const items = this.#db.getAll().filter((item) => item.parent === null);
		return this.#pagedTreeResult({ items, skip, take });
	}

	getChildrenOf({ parentId, skip = 0, take = 100 }: { parentId: string; skip?: number; take?: number }) {
		const items = this.#db.getAll().filter((item) => item.parent?.id === parentId);
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
			const children = this.#db.getAll().filter((child) => child.parent?.id === item.id);
			return {
				...item,
				hasChildren: children.length > 0,
			};
		});
		return { items: treeItemsHasChildren, total: paged.total };
	}

	move(ids: Array<string>, destinationId: string) {
		const destinationItem = this.#db.read(destinationId);
		if (!destinationItem) throw new Error(`Destination item with id ${destinationId} not found`);

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

		movedItems.forEach((movedItem: any) => this.#db.update(movedItem.id, movedItem));
		destinationItem.hasChildren = true;
		this.#db.update(destinationItem.id, destinationItem);
	}

	copy(ids: Array<string>, destinationId: string) {
		const destinationItem = this.#db.read(destinationId);
		if (!destinationItem) throw new Error(`Destination item with id ${destinationId} not found`);

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
				parentId: destinationId,
			};
		});

		copyItems.forEach((copyItem) => this.#db.create(copyItem));
		const newIds = copyItems.map((item) => item.id);

		destinationItem.hasChildren = true;
		this.#db.update(destinationItem.id, destinationItem);

		return newIds;
	}
}
