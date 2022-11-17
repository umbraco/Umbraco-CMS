import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import { MemberTypeResource, ApiError, EntityTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';
import type { MemberTypeDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMemberTypeStore
 * @extends {UmbDataStoreBase<MemberTypeDetails | EntityTreeItem>}
 * @description - Data Store for Member Types
 */
export class UmbMemberTypeStore extends UmbDataStoreBase<MemberTypeDetails | EntityTreeItem> {
	getTreeRoot(): Observable<Array<EntityTreeItem>> {
		MemberTypeResource.getTreeMemberTypeRoot({}).then(
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
