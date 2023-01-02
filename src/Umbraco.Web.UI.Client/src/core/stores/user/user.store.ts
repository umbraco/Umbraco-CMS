import { BehaviorSubject, map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import type { UserDetails } from '@umbraco-cms/models';

export type UmbUserStoreItemType = UserDetails;

/**
 * @export
 * @class UmbUserStore
 * @extends {UmbDataStoreBase<UserDetails>}
 * @description - Data Store for Users
 */
export class UmbUserStore extends UmbDataStoreBase<UmbUserStoreItemType> {


	public readonly storeAlias = 'umbUserStore';


	private _totalUsers: BehaviorSubject<number> = new BehaviorSubject(0);
	public readonly totalUsers: Observable<number> = this._totalUsers.asObservable();

	getAll(): Observable<Array<UmbUserStoreItemType>> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this._totalUsers.next(data.total);
				this.updateItems(data.items);
			});

		return this.items;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | null>)}
	 * @memberof UmbDataTypeStore
	 */
	getByKey(key: string): Observable<UmbUserStoreItemType | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems([data]);
			});

		return this.items.pipe(
			map((items: Array<UmbUserStoreItemType>) => items.find((node: UmbUserStoreItemType) => node.key === key) || null)
		);
	}

	getByKeys(keys: Array<string>): Observable<Array<UmbUserStoreItemType>> {
		const params = keys.map((key) => `key=${key}`).join('&');
		fetch(`/umbraco/backoffice/users/getByKeys?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map((items: Array<UmbUserStoreItemType>) => items.filter((node: UmbUserStoreItemType) => keys.includes(node.key)))
		);
	}

	getByName(name: string): Observable<Array<UmbUserStoreItemType>> {
		name = name.trim();
		name = name.toLocaleLowerCase();

		const params = `name=${name}`;
		fetch(`/umbraco/backoffice/users/getByName?${params}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map((items: Array<UserDetails>) =>
				items.filter((node: UserDetails) => node.name.toLocaleLowerCase().includes(name))
			)
		);
	}

	async enableUsers(userKeys: Array<string>): Promise<void> {
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
			const storedUsers = this._items.getValue().filter((user) => enabledKeys.includes(user.key));

			storedUsers.forEach((user) => {
				user.status = 'enabled';
			});

			this.updateItems(storedUsers);
		} catch (error) {
			console.error('Enable Users failed', error);
		}
	}

	async updateUserGroup(userKeys: Array<string>, userGroup: string): Promise<void> {
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
			const storedUsers = this._items.getValue().filter((user) => enabledKeys.includes(user.key));

			storedUsers.forEach((user) => {
				if (userKeys.includes(user.key)) {
					user.userGroups.push(userGroup);
				} else {
					user.userGroups = user.userGroups.filter((group) => group !== userGroup);
				}
			});

			this.updateItems(storedUsers);
		} catch (error) {
			console.error('Add user group failed', error);
		}
	}

	async removeUserGroup(userKeys: Array<string>, userGroup: string): Promise<void> {
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
			const storedUsers = this._items.getValue().filter((user) => enabledKeys.includes(user.key));

			storedUsers.forEach((user) => {
				user.userGroups = user.userGroups.filter((group) => group !== userGroup);
			});

			this.updateItems(storedUsers);
			
		} catch (error) {
			console.error('Remove user group failed', error);
		}
	}

	async disableUsers(userKeys: Array<string>): Promise<void> {
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
			const storedUsers = this._items.getValue().filter((user) => disabledKeys.includes(user.key));

			storedUsers.forEach((user) => {
				user.status = 'disabled';
			});

			this.updateItems(storedUsers);
		} catch (error) {
			console.error('Disable Users failed', error);
		}
	}

	async deleteUsers(userKeys: Array<string>): Promise<void> {
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
			this.deleteItems(deletedKeys);
		} catch (error) {
			console.error('Delete Users failed', error);
		}
	}

	async save(users: Array<UmbUserStoreItemType>): Promise<void> {
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
			this.updateItems(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}

	async invite(name: string, email: string, message: string, userGroups: Array<string>): Promise<UmbUserStoreItemType | null> {
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
			this.updateItems(json);
			return json[0];
		} catch (error) {
			console.error('Invite user error', error);
		}

		return null;
	}

	// public updateUser(user: UserItem) {
	// 	const users = this._users.getValue();
	// 	const index = users.findIndex((u) => u.key === user.key);
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
	// 		key: uuidv4(),
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

	// public deleteUser(key: string) {
	// 	const users = this._users.getValue();
	// 	const index = users.findIndex((u) => u.key === key);
	// 	if (index === -1) return;
	// 	users.splice(index, 1);
	// 	this._users.next(users);
	// 	this.requestUpdate('users');
	// }
}
