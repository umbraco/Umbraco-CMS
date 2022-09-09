import { map, Observable } from 'rxjs';
import type { UmbEntityStore } from '../../core/stores/entity.store';
import { Entity } from '../../mocks/data/entities';

export interface UmbTreeDataContext {
	rootKey: string;
	entityStore: UmbEntityStore;
	rootChanges?(key: string): Observable<Entity[]>;
	childrenChanges?(key: string): Observable<Entity[]>;
}

export class UmbTreeDataContextBase implements UmbTreeDataContext {
	public entityStore: UmbEntityStore;
	public rootKey = '';

	constructor(entityStore: UmbEntityStore) {
		this.entityStore = entityStore;
	}

	public rootChanges() {
		return this.entityStore.items.pipe(
			map((items) => items.filter((item) => item.key === this.rootKey && !item.isTrashed))
		);
	}

	public childrenChanges(key: string) {
		return this.entityStore.items.pipe(
			map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed))
		);
	}
}
