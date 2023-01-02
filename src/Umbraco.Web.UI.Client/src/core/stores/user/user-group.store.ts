import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import type { UserGroupDetails, UserGroupEntity } from '@umbraco-cms/models';

// TODO: get rid of this type addition & { ... }:
export type UmbUserGroupStoreItemType = UserGroupDetails & { users?: Array<string> };

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbDataStoreBase<UserGroupEntity>}
 * @description - Data Store for Users
 */
export class UmbUserGroupStore extends UmbDataStoreBase<UmbUserGroupStoreItemType> {


	public readonly storeAlias = 'umbUserGroupStore';

	getAll(): Observable<Array<UmbUserGroupStoreItemType>> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data.items);
			});

		return this.items;
	}

	getByKey(key: string): Observable<UmbUserGroupStoreItemType | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems([data]);
			});

		return this.items.pipe(
			map(
				(userGroups: Array<UmbUserGroupStoreItemType>) =>
					userGroups.find((userGroup: UmbUserGroupStoreItemType) => userGroup.key === key) || null
			)
		);
	}

	getByKeys(keys: Array<string>): Observable<Array<UserGroupEntity>> {
		const params = keys.map((key) => `key=${key}`).join('&');
		fetch(`/umbraco/backoffice/user-groups/getByKeys?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map((items: Array<UmbUserGroupStoreItemType>) => items.filter((node: UmbUserGroupStoreItemType) => keys.includes(node.key)))
		);
	}

	async save(userGroups: Array<UmbUserGroupStoreItemType>): Promise<void> {
		// TODO: use Fetcher API.

		// TODO: implement so user group store updates the 
		/*
		if (this._userStore && userGroup.users) {
			await this._userStore.updateUserGroup(userGroup.users, userGroup.key);
		}
		*/

		try {
			const res = await fetch('/umbraco/backoffice/user-groups/save', {
				method: 'POST',
				body: JSON.stringify(userGroups),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.updateItems(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}
}
