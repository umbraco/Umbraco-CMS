import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore'
);

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbEntityTreeStore {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
