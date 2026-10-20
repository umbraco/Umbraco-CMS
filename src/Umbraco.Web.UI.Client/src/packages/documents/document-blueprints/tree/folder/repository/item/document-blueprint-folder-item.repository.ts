import { UmbDocumentBlueprintFolderItemServerDataSource } from './document-blueprint-folder-item.server.data-source.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_CONTEXT } from './document-blueprint-folder-item.store.context-token.js';
import type { UmbDocumentBlueprintFolderItemModel } from './types.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentBlueprintFolderItemRepository extends UmbItemRepositoryBase<UmbDocumentBlueprintFolderItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintFolderItemServerDataSource, UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_CONTEXT);
	}
}

export { UmbDocumentBlueprintFolderItemRepository as api };
