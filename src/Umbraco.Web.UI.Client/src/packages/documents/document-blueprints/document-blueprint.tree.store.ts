import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore',
);

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbEntityTreeStore {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT.toString());
	}
}
