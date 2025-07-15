import { UMB_DOCUMENT_TREE_STORE_CONTEXT } from './document-tree.store.context-token.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDocumentTreeStore
 * @augments {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Items
 */
export class UmbDocumentTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbDocumentTreeStore;
