import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbDocumentTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Templates
 */
export class UmbDocumentTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>(
	'UmbDocumentTreeStore'
);
