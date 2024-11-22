import type { UmbMockDBBase } from '../mock-db-base.js';

export class UmbMockCultureItemManager<T extends { isoCode: string }> {
	#db: UmbMockDBBase<T>;
	#itemReadMapper: (item: T) => any;

	constructor(db: UmbMockDBBase<T>, itemReadMapper: (item: T) => any) {
		this.#db = db;
		this.#itemReadMapper = itemReadMapper;
	}

	getItems(isoCodes: Array<string>) {
		const items = this.#db.getAll().filter((item) => isoCodes.includes(item.isoCode));
		return items.map((item) => this.#itemReadMapper(item));
	}
}
