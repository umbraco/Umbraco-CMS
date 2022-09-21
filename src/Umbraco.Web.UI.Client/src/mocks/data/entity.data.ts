import { UmbData } from './data';
import { Entity, entities } from './entities';

// Temp mocked database
export class UmbEntityData<T extends Entity> extends UmbData<T> {
	constructor(data: Array<T>) {
		super(data);
	}

	getItems(type = '', parentKey = '') {
		if (!type) return [];
		return entities.filter((item) => item.type === type && item.parentKey === parentKey);
	}

	getByKey(key: string) {
		return this.data.find((item) => item.key === key);
	}

	save(saveItems: Array<T>) {
		saveItems.forEach((saveItem) => {
			const foundIndex = this.data.findIndex((item) => item.key === saveItem.key);
			if (foundIndex !== -1) {
				// update
				this.data[foundIndex] = saveItem;
				this.updateData(saveItem);
			} else {
				// new
				this.data.push(saveItem);
			}
		});

		return saveItems;
	}

	trash(keys: Array<string>) {
		const trashedItems: Array<T> = [];

		keys.forEach((key) => {
			const item = this.getByKey(key);
			if (!item) return;

			item.isTrashed = true;
			this.updateData(item);
			trashedItems.push(item);
		});

		return trashedItems;
	}

	protected updateData(updateItem: T) {
		const itemIndex = this.data.findIndex((item) => item.key === updateItem.key);
		const item = this.data[itemIndex];
		if (!item) return;

		const itemKeys = Object.keys(item);
		const newItem = {};

		for (const [key] of Object.entries(updateItem)) {
			if (itemKeys.indexOf(key) !== -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				newItem[key] = updateItem[key];
			}
		}

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.data[itemIndex] = newItem;
	}
}

export const umbEntityData = new UmbEntityData<Entity>(entities);
