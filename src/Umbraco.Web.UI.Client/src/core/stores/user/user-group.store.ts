import { BehaviorSubject, map, Observable } from 'rxjs';
import type { UserDetails, UserEntity, UserGroupDetails, UserGroupEntity } from '../../models';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import { v4 as uuidv4 } from 'uuid';

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbDataStoreBase<UserGroupEntity>}
 * @description - Data Store for Users
 */
export class UmbUserGroupStore extends UmbDataStoreBase<UserGroupDetails> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getAll(): Observable<Array<UserGroupDetails>> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data.items);
			});

		return this.items;
	}

	getByKeys(keys: Array<string>): Observable<Array<UserGroupEntity>> {
		const params = keys.map((key) => `key=${key}`).join('&');
		fetch(`/umbraco/backoffice/user-groups/getByKeys?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.update([data]);
			});

		return this.items.pipe(
			map((items: Array<UserGroupDetails>) => items.filter((node: UserGroupDetails) => keys.includes(node.key)))
		);
	}
}
