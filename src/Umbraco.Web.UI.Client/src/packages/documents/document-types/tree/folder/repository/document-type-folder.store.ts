import { UMB_DOCUMENT_TYPE_FOLDER_STORE_CONTEXT } from './document-type-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDocumentTypeStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentTypeFolderStore as api };
