import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore'
);

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbTreeStoreBase {
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
