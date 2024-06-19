import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentBlueprintTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintTreeStore as api };

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore',
);
