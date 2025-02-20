import { UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_CONTEXT } from './document-blueprint-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDocumentBlueprintStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Data Types
 */
export class UmbDocumentBlueprintFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbDocumentBlueprintStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintFolderStore as api };
