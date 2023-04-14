import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbDocumentTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for Templates
 */
export class UmbDocumentTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');
