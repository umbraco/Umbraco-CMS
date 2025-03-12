import { UMB_USER_PERMISSION_DOCUMENT_VALUE_SEE } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.DocumentValue.See',
		name: 'See Document Value User Permission',
		forEntityTypes: ['document-value'],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_VALUE_SEE],
			label: 'See',
			description: 'See Document value',
		},
	},
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.DocumentValue',
		name: 'Document Values Granular User Permission',
		element: () =>
			import(
				'./input-document-value-granular-user-permission/input-document-value-granular-user-permission.element.js'
			),
		meta: {
			schemaType: 'DocumentValuePermissionPresentationModel',
			label: 'Document Values',
			description: 'Assign Permissions to Document values',
		},
	},
];
