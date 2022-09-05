import { Observable, map } from 'rxjs';
import type { Entity } from '../../mocks/data/entity.data';
import { UmbDataStoreBase } from './store';

export class UmbEntityStore extends UmbDataStoreBase<Entity> {
	getByKeys(keys: Array<string>): Observable<Array<Entity>> {
		return this.items.pipe(map((items: Array<Entity>) => items.filter((item: Entity) => keys.includes(item.key))));
	}
}
