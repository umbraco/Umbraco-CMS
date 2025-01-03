import { UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Document User Permission Condition',
		alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
		api: () => import('./document-user-permission.condition.js'),
	},
];
