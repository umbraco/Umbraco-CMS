import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_TREE_STORE_CONTEXT } from './document-tree.store.context-token.js';

/**
 * @export
 * @class UmbDocumentTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Items
 */
export class UmbDocumentTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbDocumentTreeStore;
