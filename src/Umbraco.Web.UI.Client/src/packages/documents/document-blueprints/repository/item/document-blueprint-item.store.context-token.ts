import type { UmbDocumentBlueprintItemStore } from './document-blueprint-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintItemStore>(
	'UmbDocumentBlueprintItemStore',
);
