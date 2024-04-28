import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is not trashed Condition',
	alias: 'Umb.Condition.IsNotTrashed',
	api: () => import('./is-not-trashed.condition.js'),
};
