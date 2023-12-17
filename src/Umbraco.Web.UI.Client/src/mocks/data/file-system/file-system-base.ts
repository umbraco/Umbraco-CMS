import { UmbData } from '../data.js';

export abstract class UmbFileSystemMockDbBase<T extends { path: string }> extends UmbData<T> {
	constructor(data: Array<T>) {
		super(data);
	}

	create(item: T) {
		this.data.push(item);
	}

	read(path: string) {
		return this.data.find((item) => item.path === path);
	}

	update(updateItem: T) {
		const itemIndex = this.data.findIndex((item) => item.path === updateItem.path);
		this.data[itemIndex] = updateItem;
	}

	delete(path: string) {
		this.data = this.data.filter((item) => item.path !== path);
	}
}
