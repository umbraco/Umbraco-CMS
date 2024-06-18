import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbDocumentTypeTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Types
 */
export class UmbDocumentTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTypeTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentTypeTreeStore as api };

export const UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeTreeStore>(
	'UmbDocumentTypeTreeStore',
);
