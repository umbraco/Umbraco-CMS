import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Unlock Action Condition',
		alias: 'Umb.Condition.User.AllowUnlockAction',
		api: () => import('./user-allow-unlock-action.condition.js'),
	},
];
