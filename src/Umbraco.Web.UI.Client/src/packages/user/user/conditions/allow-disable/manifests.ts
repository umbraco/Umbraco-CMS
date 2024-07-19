import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Disable Action Condition',
		alias: 'Umb.Condition.User.AllowDisableAction',
		api: () => import('./user-allow-disable-action.condition.js'),
	},
];
