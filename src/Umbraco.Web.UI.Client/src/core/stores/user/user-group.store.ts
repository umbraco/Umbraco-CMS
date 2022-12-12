import { Observable } from 'rxjs';
import type { UserGroupDetails } from '../../models';
import { UmbDataStoreBase } from '../store';

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbDataStoreBase<UserGroupEntity>}
 * @description - Data Store for Users
 */
export class UmbUserGroupStore extends UmbDataStoreBase<UserGroupDetails> {
	getAll(): Observable<Array<UserGroupDetails>> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/user-groups/list/items`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data.items);
			});

		return this.items;
	}
}
