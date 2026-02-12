import { UMB_DOCUMENT_IS_TRASHED_CONDITION_ALIAS } from './constants.js';
import { UmbDocumentIsTrashedWorkspaceCondition } from './document-is-trashed.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document is trashed Workspace Condition',
	alias: UMB_DOCUMENT_IS_TRASHED_CONDITION_ALIAS,
	api: UmbDocumentIsTrashedWorkspaceCondition,
};
