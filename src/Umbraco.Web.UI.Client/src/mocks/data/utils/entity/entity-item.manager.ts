import type { UmbData } from '../../data.js';

export class UmbMockEntityItemManager<T extends { id: string }> {
	#db: UmbData<T>;
	#itemReadMapper: (item: T) => any;

	constructor(db: UmbData<T>, itemReadMapper: (item: T) => any) {
		this.#db = db;
		this.#itemReadMapper = itemReadMapper;
	}

	getItems(ids: Array<string>) {
		const items = this.#db.getData().filter((item) => ids.includes(item.id));
		return items.map((item) => this.#itemReadMapper(item));
	}
}
