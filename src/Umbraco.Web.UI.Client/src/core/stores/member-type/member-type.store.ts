import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import { MemberTypeResource, ApiError, EntityTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';
import type { MemberTypeDetails } from '@umbraco-cms/models';

export type UmbMemberTypeStoreItemType = MemberTypeDetails | EntityTreeItem

/**
 * @export
 * @class UmbMemberTypeStore
 * @extends {UmbDataStoreBase<MemberTypeDetails | EntityTreeItem>}
 * @description - Data Store for Member Types
 */
export class UmbMemberTypeStore extends UmbDataStoreBase<UmbMemberTypeStoreItemType> {

	public readonly storeAlias = 'umbMemberTypeStore';
	 
	getByKey(key: string): Observable<UmbMemberTypeStoreItemType | null> {
		return null as any;
	}

	async save(mediaTypes: Array<UmbMemberTypeStoreItemType>): Promise<void> {
		return null as any;
	}


	getTreeRoot(): Observable<Array<EntityTreeItem>> {
		MemberTypeResource.getTreeMemberTypeRoot({}).then(
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
