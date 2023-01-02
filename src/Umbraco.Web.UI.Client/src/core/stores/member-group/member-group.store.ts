import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../store';
import { ApiError, EntityTreeItem, MemberGroupResource, ProblemDetails } from '@umbraco-cms/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';

export type UmbMemberGroupStoreItemType = MemberGroupDetails | EntityTreeItem;

/**
 * @export
 * @class UmbMemberGroupStore
 * @extends {UmbDataStoreBase<MemberGroupDetails | EntityTreeItem>}
 * @description - Data Store for Member Groups
 */
export class UmbMemberGroupStore extends UmbNodeStoreBase<UmbMemberGroupStoreItemType> {

	public readonly storeAlias = 'umbMemberGroupStore';

	getByKey(key: string): Observable<UmbMemberGroupStoreItemType | null> {
		return null as any;
	}

	async save(mediaTypes: Array<UmbMemberGroupStoreItemType>): Promise<void> {
		return null as any;
	}

	getTreeRoot(): Observable<Array<EntityTreeItem>> {
		MemberGroupResource.getTreeMemberGroupRoot({}).then(
			(res) => {
				this.updateItems(res.items);
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
