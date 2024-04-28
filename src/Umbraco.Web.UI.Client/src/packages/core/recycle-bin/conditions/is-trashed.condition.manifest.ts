import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Enable Action Condition',
	alias: 'Umb.Condition.User.AllowEnableAction',
	api: () => import('./is-trashed.condition.js'),
};
