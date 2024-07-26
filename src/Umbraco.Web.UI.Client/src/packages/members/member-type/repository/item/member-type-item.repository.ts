import { UmbMemberTypeItemServerDataSource } from './member-type-item.server.data-source.js';
import { UMB_MEMBER_TYPE_ITEM_STORE_CONTEXT } from './member-type-item.store.context-token.js';
import type { UmbMemberTypeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMemberTypeItemRepository extends UmbItemRepositoryBase<UmbMemberTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeItemServerDataSource, UMB_MEMBER_TYPE_ITEM_STORE_CONTEXT);
	}
}

export default UmbMemberTypeItemRepository;
