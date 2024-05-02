import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document is trashed Workspace Condition',
	alias: 'Umb.Condition.Workspace.DocumentIsTrashed',
	api: () => import('./document-is-trashed.condition.js'),
};
