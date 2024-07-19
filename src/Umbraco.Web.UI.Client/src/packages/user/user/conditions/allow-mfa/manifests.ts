import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Mfa Action Condition',
		alias: 'Umb.Condition.User.AllowMfaAction',
		api: () => import('./user-allow-mfa-action.condition.js'),
	},
];
