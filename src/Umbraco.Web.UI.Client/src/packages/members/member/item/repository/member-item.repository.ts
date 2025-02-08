import { UmbMemberItemServerDataSource } from './member-item.server.data-source.js';
import { UMB_MEMBER_ITEM_STORE_CONTEXT } from './member-item.store.context-token.js';
import type { UmbMemberItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMemberItemRepository extends UmbItemRepositoryBase<UmbMemberItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberItemServerDataSource, UMB_MEMBER_ITEM_STORE_CONTEXT);
	}
}

export default UmbMemberItemRepository;
