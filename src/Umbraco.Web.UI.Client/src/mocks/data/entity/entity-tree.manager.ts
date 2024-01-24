import { UmbEntityMockDbBase } from './entity-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockEntityTreeManager<T extends Omit<EntityTreeItemResponseModel, 'type'>> {
	#db: UmbEntityMockDbBase<T>;
	#treeItemMapper: (item: T) => any;

	constructor(mockDb: UmbEntityMockDbBase<T>, treeItemMapper: (item: T) => any) {
		this.#db = mockDb;
		this.#treeItemMapper = treeItemMapper;
	}

	getRoot() {
		const items = this.#db.getData().filter((item) => item.parentId === null);
		const treeItems = items.map((item) => this.#treeItemMapper(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getChildrenOf(parentId: string) {
		const items = this.#db.getData().filter((item) => item.parentId === parentId);
		const treeItems = items.map((item) => this.#treeItemMapper(item));
		const total = items.length;
		return { items: treeItems, total };
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
