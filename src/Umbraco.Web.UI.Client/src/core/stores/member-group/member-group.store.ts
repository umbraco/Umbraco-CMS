import { map, Observable } from 'rxjs';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import { ApiError, EntityTreeItem, MemberGroupResource, ProblemDetails } from '@umbraco-cms/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMemberGroupStore
 * @extends {UmbDataStoreBase<MemberGroupDetails | EntityTreeItem>}
 * @description - Data Store for Member Groups
 */
export class UmbMemberGroupStore extends UmbDataStoreBase<MemberGroupDetails | EntityTreeItem> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getTreeRoot(): Observable<Array<EntityTreeItem>> {
		MemberGroupResource.getTreeMemberGroupRoot({}).then(
			(res) => {
				this.update(res.items);
			},
			(e) => {
				if (e instanceof ApiError) {
					const error = e.body as ProblemDetails;
					if (e.status === 400) {
						console.log(error.detail);
					}
				}
			}
		);

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}
}
