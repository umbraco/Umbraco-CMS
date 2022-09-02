import { entities } from './entities';
import { deepmerge } from 'deepmerge-ts';
import { Entity } from './entity.data';

// Temp mocked database
export class UmbData<T extends Entity> {
	private _data: Array<T> = [];

	constructor(data: Array<T>) {
		this._data = data;
	}

	getById(id: number) {
		return this._data.find((item) => item.id === id);
	}

	getByKey(key: string) {
		return this._data.find((item) => item.key === key);
	}

	save(saveItems: Array<T>) {
		saveItems.forEach((saveItem) => {
			const foundIndex = this._data.findIndex((item) => item.id === saveItem.id);
			if (foundIndex !== -1) {
				// update
				this._data[foundIndex] = saveItem;
				this._updateEntity(saveItem);
			} else {
				// new
				this._data.push(saveItem);
			}
		});

		return saveItems;
	}

	trash(key: string) {
		const item = this.getByKey(key);
		if (!item) return;

		item.isTrashed = true;
		this._updateEntity(item);
		return item;
	}

	private _updateEntity(saveItem: T) {
		const entityIndex = entities.findIndex((item) => item.key === saveItem.key);
		const entity = entities[entityIndex];
		if (!entity) return;

		const entityKeys = Object.keys(entity);
		const mergedData = deepmerge(entity, saveItem);

		for (const [key] of Object.entries(mergedData)) {
			if (entityKeys.indexOf(key) === -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				delete mergedData[key];
			}
		}

		entities[entityIndex] = mergedData;
	}
}
