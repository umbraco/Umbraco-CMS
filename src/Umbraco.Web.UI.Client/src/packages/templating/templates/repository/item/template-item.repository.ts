import { UmbTemplateItemServerDataSource } from './template-item.server.data-source.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT } from './template-item.store.context-token.js';
import type { UmbTemplateItemModel } from './types.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateItemRepository extends UmbItemRepositoryBase<UmbTemplateItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateItemServerDataSource, UMB_TEMPLATE_ITEM_STORE_CONTEXT);
	}
}

export default UmbTemplateItemRepository;
