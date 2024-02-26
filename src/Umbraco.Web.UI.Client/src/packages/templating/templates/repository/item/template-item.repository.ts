import { UmbTemplateItemServerDataSource } from './template-item.server.data-source.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT } from './template-item.store.js';
import type { UmbTemplateItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbTemplateItemRepository extends UmbItemRepositoryBase<UmbTemplateItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateItemServerDataSource, UMB_TEMPLATE_ITEM_STORE_CONTEXT);
	}
}

export default UmbTemplateItemRepository;
