import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS =
	'Umb.Modal.DocumentPropertyValueUserPermissionFlow.PropertyType';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS,
		name: 'Document Property Value User Permission Flow Property Type Modal',
		element: () => import('./property-type-modal.element.js'),
	},
];
