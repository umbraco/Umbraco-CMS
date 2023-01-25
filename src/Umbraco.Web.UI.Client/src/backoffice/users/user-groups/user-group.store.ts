import type { UserGroupDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';

// TODO: get rid of this type addition & { ... }:
export type UmbUserGroupStoreItemType = UserGroupDetails & { users?: Array<string> };

export const UMB_USER_GROUP_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserGroupStore>('UmbUserGroupStore');

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Groups
 */
export class UmbUserGroupStore extends UmbStoreBase {


	#groups = new ArrayState<UmbUserGroupStoreItemType>([], x => x.key);
	public groups = this.#groups.asObservable();


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_USER_GROUP_STORE_CONTEXT_TOKEN.toString());
	}

	getAll() {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this.#groups.append(data.items);
			});

		return this.groups;
	}

	getByKey(key: string) {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#groups.append([data]);
			});

		return createObservablePart(this.groups, (userGroups) => userGroups.find(userGroup => userGroup.key === key));
	}

	getByKeys(keys: Array<string>) {
		const params = keys.map((key) => `key=${key}`).join('&');
		fetch(`/umbraco/backoffice/user-groups/getByKeys?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.#groups.append(data);
			});

			return createObservablePart(this.groups, (items) => items.filter(node => keys.includes(node.key)));
	}

	async save(userGroups: Array<UmbUserGroupStoreItemType>) {
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
			this.#groups.append(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}
}
