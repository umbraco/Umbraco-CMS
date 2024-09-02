import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Change Password Condition',
		alias: 'Umb.Condition.User.AllowChangePassword',
		api: () => import('./user-allow-change-password-action.condition.js'),
	},
];
