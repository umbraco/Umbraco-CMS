import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDocumentBlueprintDetailStore } from './document-blueprint-detail.store.js';

export const UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintDetailStore>(
	'UmbDocumentBlueprintDetailStore',
);
