import { UmbEntityData } from './entity.data.js';
import { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbEntityTreeData<TreeItemType extends TreeItemPresentationModel> {
	#detailDatabase: UmbEntityData<TreeItemType>;

	constructor(detailDatabase: UmbEntityData<TreeItemType>) {
		this.#detailDatabase = detailDatabase;
	}

	move(ids: Array<string>, destinationId: string) {
		const destinationItem = this.#detailDatabase.getById(destinationId);
		if (!destinationItem) throw new Error(`Destination item with key ${destinationId} not found`);

		const items = this.#detailDatabase.getByIds(ids);
		const movedItems = items.map((item) => {
			return {
				...item,
				parentId: destinationId,
			};
		});

		movedItems.forEach((movedItem) => this.#detailDatabase.updateData(movedItem));
		destinationItem.hasChildren = true;
		this.#detailDatabase.updateData(destinationItem);
	}

	copy(ids: Array<string>, destinationKey: string) {
		const destinationItem = this.#detailDatabase.getById(destinationKey);
		if (!destinationItem) throw new Error(`Destination item with key ${destinationKey} not found`);

		// TODO: Notice we don't add numbers to the 'copy' name.
		const items = this.#detailDatabase.getByIds(ids);
		const copyItems = items.map((item) => {
			return {
				...item,
				name: item.name + ' Copy',
				id: UmbId.new(),
				parentId: destinationKey,
			};
		});

		copyItems.forEach((copyItem) => this.#detailDatabase.insert(copyItem));
		const newIds = copyItems.map((item) => item.id);

		destinationItem.hasChildren = true;
		this.#detailDatabase.updateData(destinationItem);

		return newIds;
	}
}
