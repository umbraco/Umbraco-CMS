// Temp mocked database
export class UmbData<T extends { id: number; key: string }> {
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

	save(data: Array<T>) {
		data.forEach((storedItem) => {
			const foundIndex = this._data.findIndex((item) => item.id === storedItem.id);
			if (foundIndex !== -1) {
				// replace
				this._data[foundIndex] = storedItem;
			} else {
				// new
				this._data.push(storedItem);
			}
		});

		return data;
	}
}
