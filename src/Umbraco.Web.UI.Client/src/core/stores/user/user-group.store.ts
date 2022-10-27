import { map, Observable } from 'rxjs';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import type { UserGroupDetails, UserGroupEntity } from '@umbraco-cms/models';

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

	getByKey(key: string): Observable<UserGroupDetails | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update([data]);
			});

		return this.items.pipe(
			map(
				(userGroups: Array<UserGroupDetails>) =>
					userGroups.find((userGroup: UserGroupDetails) => userGroup.key === key) || null
			)
		);
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
