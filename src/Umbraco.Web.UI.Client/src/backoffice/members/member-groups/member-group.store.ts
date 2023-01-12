import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../../../core/stores/store';
import { EntityTreeItem, MemberGroupResource } from '@umbraco-cms/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

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
		tryExecuteAndNotify(this.host, MemberGroupResource.getTreeMemberGroupRoot({})).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}
}
