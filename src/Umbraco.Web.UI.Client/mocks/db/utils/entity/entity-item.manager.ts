import type { UmbMockDBBase } from '../mock-db-base.js';

export class UmbMockEntityItemManager<T extends { id: string }> {
	protected _db: UmbMockDBBase<T>;
	protected _itemReadMapper: (item: T) => any;

	constructor(db: UmbMockDBBase<T>, itemReadMapper: (item: T) => any) {
		this._db = db;
		this._itemReadMapper = itemReadMapper;
	}

	getItems(ids: Array<string>) {
		const items = this._db.getAll().filter((item) => ids.includes(item.id));
		return items.map((item) => this._itemReadMapper(item));
	}
}
