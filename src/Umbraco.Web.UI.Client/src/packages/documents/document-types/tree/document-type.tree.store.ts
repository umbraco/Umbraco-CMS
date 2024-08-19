import { UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT } from './document-type.tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDocumentTypeTreeStore
 * @augments {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Types
 */
export class UmbDocumentTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTypeTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentTypeTreeStore as api };
