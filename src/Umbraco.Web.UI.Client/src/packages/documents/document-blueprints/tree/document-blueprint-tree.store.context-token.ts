import type { UmbDocumentBlueprintTreeStore } from './document-blueprint-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore',
);
