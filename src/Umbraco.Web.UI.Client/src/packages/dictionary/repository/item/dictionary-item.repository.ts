import { UmbDictionaryItemServerDataSource } from './dictionary-item.server.data-source.js';
import { UMB_DICTIONARY_ITEM_STORE_CONTEXT } from './dictionary-item.store.context-token.js';
import type { UmbDictionaryItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDictionaryItemRepository extends UmbItemRepositoryBase<UmbDictionaryItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDictionaryItemServerDataSource, UMB_DICTIONARY_ITEM_STORE_CONTEXT);
	}
}

export { UmbDictionaryItemRepository as api };
