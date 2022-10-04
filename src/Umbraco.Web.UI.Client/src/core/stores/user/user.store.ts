import { map, Observable } from 'rxjs';
import type { UserEntity } from '../../models';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';

/**
 * @export
 * @class UmbUserStore
 * @extends {UmbDataStoreBase<UserEntity>}
 * @description - Data Store for Users
 */
export class UmbUserStore extends UmbDataStoreBase<UserEntity> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getAll(): Observable<Array<UserEntity>> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data.items);
			});

		return this.items;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | null>)}
	 * @memberof UmbDataTypeStore
	 */
	getByKey(key: string): Observable<UserEntity | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/users/${key}`)
			.then((res) => res.json())
			.then((data) => {
				console.log('getByKey', data);
				this.update(data);
			});

		return this.items.pipe(
			map((dataTypes: Array<UserEntity>) => dataTypes.find((node: UserEntity) => node.key === key) || null)
		);
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
