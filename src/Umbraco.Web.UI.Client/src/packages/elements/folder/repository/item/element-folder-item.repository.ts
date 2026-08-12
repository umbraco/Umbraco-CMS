import { UmbElementFolderItemServerDataSource } from './element-folder-item.server.data-source.js';
import { UMB_ELEMENT_FOLDER_ITEM_STORE_CONTEXT } from './element-folder-item.store.context-token.js';
import type { UmbElementFolderItemModel } from './types.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementFolderItemRepository extends UmbItemRepositoryBase<UmbElementFolderItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementFolderItemServerDataSource, UMB_ELEMENT_FOLDER_ITEM_STORE_CONTEXT);
	}
}

export { UmbElementFolderItemRepository as api };
