import { pagedResult } from '../paged-result.js';
import type { UmbEntityMockDbBase } from './entity-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockEntityTreeManager<T extends Omit<EntityTreeItemResponseModel, 'type'>> {
	#db: UmbEntityMockDbBase<T>;
	#treeItemMapper: (item: T) => any;

	constructor(mockDb: UmbEntityMockDbBase<T>, treeItemMapper: (item: T) => any) {
		this.#db = mockDb;
		this.#treeItemMapper = treeItemMapper;
	}

	getRoot({ skip = 0, take = 100 }: { skip?: number; take?: number } = {}) {
		const items = this.#db.getAll().filter((item) => item.parent === null);
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => this.#treeItemMapper(item));
		return { items: treeItems, total: paged.total };
	}

	getChildrenOf({ parentId, skip = 0, take = 100 }: { parentId: string; skip?: number; take?: number }) {
		const items = this.#db.getAll().filter((item) => item.parent?.id === parentId);
		const paged = pagedResult(items, skip, take);
		const treeItems = paged.items.map((item) => this.#treeItemMapper(item));
		return { items: treeItems, total: paged.total };
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
				parentId: destinationId,
			};
		});

		movedItems.forEach((movedItem: any) => this.#db.update(movedItem.id, movedItem));
		destinationItem.hasChildren = true;
		this.#db.update(destinationItem.id, destinationItem);
	}

	copy(ids: Array<string>, destinationId: string) {
		const destinationItem = this.#db.read(destinationId);
		if (!destinationItem) throw new Error(`Destination item with id ${destinationId} not found`);

		// TODO: Notice we don't add numbers to the 'copy' name.
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
