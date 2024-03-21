import { UmbRelationTypeItemServerDataSource } from './relation-type-item.server.data-source.js';
import { UMB_RELATION_TYPE_ITEM_STORE_CONTEXT } from './relation-type-item.store.js';
import type { UmbRelationTypeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbRelationTypeItemRepository extends UmbItemRepositoryBase<UmbRelationTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbRelationTypeItemServerDataSource, UMB_RELATION_TYPE_ITEM_STORE_CONTEXT);
	}
}
