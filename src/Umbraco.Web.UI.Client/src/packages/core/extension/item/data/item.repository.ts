import { UmbExtensionItemDataSource } from './item.data-source.js';
import { UMB_EXTENSION_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbExtensionItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbExtensionItemRepository extends UmbItemRepositoryBase<UmbExtensionItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbExtensionItemDataSource, UMB_EXTENSION_ITEM_STORE_CONTEXT);
	}
}

export { UmbExtensionItemRepository as api };
