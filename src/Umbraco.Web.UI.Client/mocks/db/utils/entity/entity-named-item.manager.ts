import type { UmbMockDBBase } from '../mock-db-base.js';
import { UmbMockEntityItemManager } from './entity-item.manager.js';

export class UmbMockEntityNamedItemManager<T extends { id: string; name: string }> extends UmbMockEntityItemManager<T> {
	constructor(db: UmbMockDBBase<T>, itemReadMapper: (item: T) => any) {
		super(db, itemReadMapper);
	}

	search(query: string, skip: number = 0, take: number = 100) {
		const queryLower = query.toLowerCase();
		const filtered = this._db.getAll().filter((item) => item.name.toLowerCase().includes(queryLower));
		const items = filtered.slice(skip, skip + take).map((item) => this._itemReadMapper(item));
		return { items, total: filtered.length };
	}
}
