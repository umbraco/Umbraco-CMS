import { UmbStylesheetItemServerDataSource } from './stylesheet-item.server.data-source.js';
import { UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN } from './stylesheet-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { StylesheetItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbStylesheetItemRepository extends UmbItemRepositoryBase<StylesheetItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetItemServerDataSource, UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN);
	}
}
