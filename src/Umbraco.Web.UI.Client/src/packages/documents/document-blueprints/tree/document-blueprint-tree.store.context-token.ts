import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDocumentBlueprintTreeStore } from './document-blueprint-tree.store.js';

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore',
);
