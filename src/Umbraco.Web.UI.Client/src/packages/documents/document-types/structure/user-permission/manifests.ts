import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
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
