import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow ExternalLogin Action Condition',
		alias: 'Umb.Condition.User.AllowExternalLoginAction',
		api: () => import('./user-allow-external-login-action.condition.js'),
	},
];
