import { UmbExtensionTypeItemDataSource } from './item.data-source.js';
import { UMB_EXTENSION_TYPE_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbExtensionTypeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbExtensionTypeItemRepository extends UmbItemRepositoryBase<UmbExtensionTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbExtensionTypeItemDataSource, UMB_EXTENSION_TYPE_ITEM_STORE_CONTEXT);
	}
}

export { UmbExtensionTypeItemRepository as api };
