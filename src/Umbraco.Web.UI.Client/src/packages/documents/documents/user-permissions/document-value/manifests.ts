import { UMB_USER_PERMISSION_DOCUMENT_VALUE_READ, UMB_USER_PERMISSION_DOCUMENT_VALUE_WRITE } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.DocumentValue.Read',
		name: 'Read Document Value User Permission',
		forEntityTypes: ['document-value'],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_VALUE_READ],
			label: 'Read',
			description: 'Read Document values',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.DocumentValue.Write',
		name: 'Write Document Value User Permission',
		forEntityTypes: ['document-value'],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_VALUE_WRITE],
			label: 'Write',
			description: 'Write Document values',
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
