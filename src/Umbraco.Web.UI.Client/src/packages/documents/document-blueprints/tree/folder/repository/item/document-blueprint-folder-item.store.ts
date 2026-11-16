import type { UmbDocumentBlueprintFolderItemModel } from './types.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_CONTEXT } from './document-blueprint-folder-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbDocumentBlueprintFolderItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document Blueprint Folder items
 */
export class UmbDocumentBlueprintFolderItemStore extends UmbItemStoreBase<UmbDocumentBlueprintFolderItemModel> {
	/**
	 * Creates an instance of UmbDocumentBlueprintFolderItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintFolderItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintFolderItemStore as api };
