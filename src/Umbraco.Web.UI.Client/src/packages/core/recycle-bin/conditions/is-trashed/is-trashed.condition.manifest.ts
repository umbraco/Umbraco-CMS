import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is trashed Condition',
	alias: 'Umb.Condition.IsTrashed',
	api: () => import('./is-trashed.condition.js'),
};
