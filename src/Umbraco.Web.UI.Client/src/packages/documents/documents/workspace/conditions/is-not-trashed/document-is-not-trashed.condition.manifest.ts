import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document is not trashed Workspace Condition',
	alias: 'Umb.Condition.Workspace.DocumentIsNotTrashed',
	api: () => import('./document-is-not-trashed.condition.js'),
};
