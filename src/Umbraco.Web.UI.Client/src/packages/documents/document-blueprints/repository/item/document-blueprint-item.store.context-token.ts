import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDocumentBlueprintItemStore } from './document-blueprint-item.store.js';

export const UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintItemStore>(
	'UmbDocumentBlueprintItemStore',
);
