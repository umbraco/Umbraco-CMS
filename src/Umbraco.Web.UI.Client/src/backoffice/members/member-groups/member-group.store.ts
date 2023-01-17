import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../../../core/stores/store';
import { EntityTreeItem, MemberGroupResource } from '@umbraco-cms/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextAlias } from '@umbraco-cms/context-api';

export type UmbMemberGroupStoreItemType = MemberGroupDetails | EntityTreeItem;

export const STORE_ALIAS = 'umbMemberGroupStore';

/**
 * @export
 * @class UmbMemberGroupStore
 * @extends {UmbDataStoreBase<MemberGroupDetails | EntityTreeItem>}
 * @description - Data Store for Member Groups
 */
export class UmbMemberGroupStore extends UmbNodeStoreBase<UmbMemberGroupStoreItemType> {
	public readonly storeAlias = STORE_ALIAS;

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

export const UMB_MEMBER_GROUP_STORE_CONTEXT_ALIAS = new UmbContextAlias<UmbMemberGroupStore>(STORE_ALIAS);
