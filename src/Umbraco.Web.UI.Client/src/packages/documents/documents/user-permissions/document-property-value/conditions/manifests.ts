import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Document Property Value User Permission Condition',
		alias: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS,
		api: () => import('./document-property-value-user-permission.condition.js'),
	},
];
