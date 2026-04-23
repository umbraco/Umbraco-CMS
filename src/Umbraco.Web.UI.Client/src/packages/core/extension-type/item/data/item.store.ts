import { UMB_EXTENSION_TYPE_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbExtensionTypeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

export class UmbExtensionTypeItemStore extends UmbItemStoreBase<UmbExtensionTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_EXTENSION_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbExtensionTypeItemStore as api };
