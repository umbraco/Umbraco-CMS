import type { UserGroupDetails } from '../types';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';

// TODO: get rid of this type addition & { ... }:
//export type UmbUserGroupStoreItemType = UserGroupDetails & { users?: Array<string> };

export const UMB_USER_GROUP_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserGroupStore>('UmbUserGroupStore');

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Groups
 */
export class UmbUserGroupStore extends UmbStoreBase implements UmbEntityDetailStore<UserGroupDetails> {
	#groups = new UmbArrayState<UserGroupDetails>([], (x) => x.id);
	public groups = this.#groups.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<UserGroupDetails>([], (x) => x.id));
	}

	getScaffold(entityType: string, parentId: string | null) {
		return {
			id: '',
			name: '',
			icon: '',
			type: 'user-group',
			hasChildren: false,
			parentId: '',
			sections: [],
			permissions: [],
			users: [],
		} as UserGroupDetails;
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

	getByKey(id: string) {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/details/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this.#groups.append([data]);
			});

		return this.#groups.getObservablePart((userGroups) => userGroups.find((userGroup) => userGroup.id === id));
	}

	async save(userGroups: Array<UserGroupDetails>) {
		// TODO: use Fetcher API.

		// TODO: implement so user group store updates the users, but these needs to save as well..?
		/*
		if (this._userStore && userGroup.users) {
			await this._userStore.updateUserGroup(userGroup.users, userGroup.id);
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
