import { UMB_USER_PERMISSION_DOCUMENT_TYPE_STRUCTURE_PROPERTY_SEE } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.DocumentType.Structure.Property.See',
		name: 'See Document Type Structure Property User Permission',
		forEntityTypes: ['document-type-property'],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_TYPE_STRUCTURE_PROPERTY_SEE],
			label: 'See',
			description: 'See Document Type Property',
		},
	},
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.ContentType.Structure',
		name: 'Content Type Structure Granular User Permission',
		element: () =>
			import(
				'./input-document-type-structure-granular-user-permission/input-document-type-structure-granular-user-permission.element.js'
			),
		meta: {
			schemaType: 'DocumentTypeStructurePermissionPresentationModel',
			label: 'Document Type Structure',
			description: 'Assign Permissions to Document Type structures',
		},
	},
];
