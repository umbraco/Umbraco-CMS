import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'Document User Permission Condition',
		alias: 'Umb.Condition.UserPermission.Document',
		api: () => import('./document-user-permission.condition.js'),
	},
];
