import { UMB_DOCUMENT_IS_NOT_TRASHED_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document is not trashed Workspace Condition',
	alias: UMB_DOCUMENT_IS_NOT_TRASHED_CONDITION_ALIAS,
	api: () => import('./document-is-not-trashed.condition.js'),
};
