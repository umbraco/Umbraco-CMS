import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Content has properties Workspace Condition',
	alias: 'Umb.Condition.Workspace.ContentHasProperties',
	api: () => import('./content-has-properties.condition.js'),
};
