import type { UmbStylesheetItemModel } from '../../types.js';
import { UmbStylesheetItemServerDataSource } from './stylesheet-item.server.data-source.js';
import { UMB_STYLESHEET_ITEM_STORE_CONTEXT } from './stylesheet-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbStylesheetItemRepository extends UmbItemRepositoryBase<UmbStylesheetItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetItemServerDataSource, UMB_STYLESHEET_ITEM_STORE_CONTEXT);
	}
}
