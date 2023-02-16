import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

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
	UmbDocumentTreeStore.name
);
