import { UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT } from './document-blueprint-tree.store.context-token.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @augments {UmbStoreBase}
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
