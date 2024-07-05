import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Enable Action Condition',
		alias: 'Umb.Condition.User.AllowEnableAction',
		api: () => import('./user-allow-enable-action.condition.js'),
	},
];
