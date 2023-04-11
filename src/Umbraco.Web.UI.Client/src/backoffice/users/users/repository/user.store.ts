import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { ArrayState, NumberState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export type UmbUserStoreItemType = UserDetails;

export const UMB_USER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserStore>('UmbUserStore');

/**
 * @export
 * @class UmbUserStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Users
 */
export class UmbUserStore extends UmbStoreBase implements UmbEntityDetailStore<UserDetails> {
	#users = new ArrayState<UserDetails>([], (x) => x.id);
	public users = this.#users.asObservable();

	#totalUsers = new NumberState(0);
	public readonly totalUsers = this.#totalUsers.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_STORE_CONTEXT_TOKEN.toString());
	}

	getScaffold(entityType: string, parentId: string | null) {
		return {
			id: '',
			name: '',
			icon: '',
			type: 'user',
			hasChildren: false,
			parentId: '',
			email: '',
			language: '',
			status: 'enabled',
			updateDate: '8/27/2022',
			createDate: '9/19/2022',
			failedLoginAttempts: 0,
			userGroups: [],
			contentStartNodes: [],
			mediaStartNodes: [],
		} as UserDetails;
	}

	getAll() {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this.#totalUsers.next(data.total);
				this.#users.next(data.items);
			});

		return this.users;
	}

	/**
	 * @description - Request a User by id. The User is added to the store and is returned as an Observable.
	 * @param {string} id
	 * @return {*}  {(Observable<DataTypeModel | null>)}
	 * @memberof UmbDataTypeStore
	 */
	getByKey(id: string) {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users/details/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this.#users.appendOne(data);
			});

		return this.#users.getObservablePart((users: Array<UmbUserStoreItemType>) =>
			users.find((user: UmbUserStoreItemType) => user.id === id)
		);
	}

	/**
	 * @description - Request Users by ids.
	 * @param {string} id
	 * @return {*}  {(Observable<UserDetails | null>)}
	 * @memberof UmbDataTypeStore
	 */
	getByKeys(ids: Array<string>) {
		const params = ids.map((id) => `id=${id}`).join('&');
		fetch(`/umbraco/backoffice/users/getByKeys?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.#users.append(data);
			});

		return this.#users.getObservablePart((users: Array<UmbUserStoreItemType>) =>
			users.filter((user: UmbUserStoreItemType) => ids.includes(user.id))
		);
	}

	getByName(name: string) {
		name = name.trim();
		name = name.toLocaleLowerCase();

		const params = `name=${name}`;
		fetch(`/umbraco/backoffice/users/getByName?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.#users.append(data);
			});

		return this.#users.getObservablePart((users: Array<UmbUserStoreItemType>) =>
			users.filter((user: UmbUserStoreItemType) => user.name.toLocaleLowerCase().includes(name))
		);
	}

	async enableUsers(userKeys: Array<string>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/enable', {
				method: 'POST',
				body: JSON.stringify(userKeys),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const enabledKeys = await res.json();
			const storedUsers = this.#users.getValue().filter((user) => enabledKeys.includes(user.id));

			storedUsers.forEach((user) => {
				user.status = 'enabled';
			});

			this.#users.append(storedUsers);
		} catch (error) {
			console.error('Enable Users failed', error);
		}
	}

	async updateUserGroup(userKeys: Array<string>, userGroup: string) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/updateUserGroup', {
				method: 'POST',
				body: JSON.stringify({ userKeys, userGroup }),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const enabledKeys = await res.json();
			const storedUsers = this.#users.getValue().filter((user) => enabledKeys.includes(user.id));

			storedUsers.forEach((user) => {
				if (userKeys.includes(user.id)) {
					user.userGroups.push(userGroup);
				} else {
					user.userGroups = user.userGroups.filter((group) => group !== userGroup);
				}
			});

			this.#users.append(storedUsers);
		} catch (error) {
			console.error('Add user group failed', error);
		}
	}

	async removeUserGroup(userKeys: Array<string>, userGroup: string) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/enable', {
				method: 'POST',
				body: JSON.stringify({ userKeys, userGroup }),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const enabledKeys = await res.json();
			const storedUsers = this.#users.getValue().filter((user) => enabledKeys.includes(user.id));

			storedUsers.forEach((user) => {
				user.userGroups = user.userGroups.filter((group) => group !== userGroup);
			});

			this.#users.append(storedUsers);
		} catch (error) {
			console.error('Remove user group failed', error);
		}
	}

	async disableUsers(userKeys: Array<string>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/disable', {
				method: 'POST',
				body: JSON.stringify(userKeys),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const disabledKeys = await res.json();
			const storedUsers = this.#users.getValue().filter((user) => disabledKeys.includes(user.id));

			storedUsers.forEach((user) => {
				user.status = 'disabled';
			});

			this.#users.append(storedUsers);
		} catch (error) {
			console.error('Disable Users failed', error);
		}
	}

	async deleteUsers(userKeys: Array<string>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/delete', {
				method: 'POST',
				body: JSON.stringify(userKeys),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const deletedKeys = await res.json();
			this.#users.remove(deletedKeys);
		} catch (error) {
			console.error('Delete Users failed', error);
		}
	}

	async save(users: Array<UmbUserStoreItemType>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/save', {
				method: 'POST',
				body: JSON.stringify(users),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.#users.append(json);
		} catch (error) {
			console.error('Save user error', error);
		}
	}

	async invite(name: string, email: string, message: string, userGroups: Array<string>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/users/invite', {
				method: 'POST',
				body: JSON.stringify({ name, email, message, userGroups }),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = (await res.json()) as UmbUserStoreItemType[];
			this.#users.append(json);
			return json[0];
		} catch (error) {
			console.error('Invite user error', error);
		}

		return null;
	}

	// public updateUser(user: UserItem) {
	// 	const users = this._users.getValue();
	// 	const index = users.findIndex((u) => u.id === user.id);
	// 	if (index === -1) return;
	// 	users[index] = { ...users[index], ...user };
	// 	console.log('updateUser', user, users[index]);
	// 	this._users.next(users);
	// 	this.requestUpdate('users');
	// }

	// public inviteUser(name: string, email: string, userGroup: string, message: string): UserItem {
	// 	const users = this._users.getValue();
	// 	const user = {
	// 		id: this._users.getValue().length + 1,
	// 		id: uuidv4(),
	// 		name: name,
	// 		email: email,
	// 		status: 'invited',
	// 		language: 'en',
	// 		updateDate: new Date().toISOString(),
	// 		createDate: new Date().toISOString(),
	// 		failedLoginAttempts: 0,
	// 		userGroup: userGroup,
	// 	};
	// 	this._users.next([...users, user]);
	// 	this.requestUpdate('users');

	// 	//TODO: Send invite email with message
	// 	return user;
	// }

	// public deleteUser(id: string) {
	// 	const users = this._users.getValue();
	// 	const index = users.findIndex((u) => u.id === id);
	// 	if (index === -1) return;
	// 	users.splice(index, 1);
	// 	this._users.next(users);
	// 	this.requestUpdate('users');
	// }
}
