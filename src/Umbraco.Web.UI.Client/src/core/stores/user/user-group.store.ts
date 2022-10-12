import { BehaviorSubject, map, Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import type { UserDetails, UserEntity, UserGroupDetails } from '../../models';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';

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
}
