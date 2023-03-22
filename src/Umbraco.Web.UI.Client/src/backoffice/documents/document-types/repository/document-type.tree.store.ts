import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbDocumentTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document-Types
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbDocumentTypeTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbDocumentTypeTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentTypeTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeTreeStore>(
	'UmbDocumentTypeTreeStore'
);
