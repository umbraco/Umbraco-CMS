import type { UmbMockDBBase } from '../mock-db-base.js';

export class UmbMockEntityItemManager<T extends { id: string }> {
	#db: UmbMockDBBase<T>;
	#itemReadMapper: (item: T) => any;

	constructor(db: UmbMockDBBase<T>, itemReadMapper: (item: T) => any) {
		this.#db = db;
		this.#itemReadMapper = itemReadMapper;
	}

	getItems(ids: Array<string>) {
		const items = this.#db.getAll().filter((item) => ids.includes(item.id));
		return items.map((item) => this.#itemReadMapper(item));
	}
}
