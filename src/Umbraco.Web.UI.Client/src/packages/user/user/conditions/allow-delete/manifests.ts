import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Delete Action Condition',
		alias: 'Umb.Condition.User.AllowDeleteAction',
		api: () => import('./user-allow-delete-action.condition.js'),
	},
];
