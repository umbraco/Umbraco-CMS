import { UmbMemberGroupItemServerDataSource } from './member-group-item.server.data-source.js';
import { UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT } from './member-group-item.store.context-token.js';
import type { UmbMemberGroupItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMemberGroupItemRepository extends UmbItemRepositoryBase<UmbMemberGroupItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberGroupItemServerDataSource, UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT);
	}
}

export default UmbMemberGroupItemRepository;
