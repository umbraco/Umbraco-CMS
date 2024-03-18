import type { UmbDocumentNavigationStructureContext } from './document-navigation-structure.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_NAVIGATION_STRUCTURE_CONTEXT = new UmbContextToken<UmbDocumentNavigationStructureContext>(
	'UmbDocumentNavigationStructureContext',
);
