import { UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT } from './document-recycle-bin-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDocumentRecycleBinTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Document Recycle Bin Tree Items
 */
export class UmbDocumentRecycleBinTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentRecycleBinTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentRecycleBinTreeStore as api };
