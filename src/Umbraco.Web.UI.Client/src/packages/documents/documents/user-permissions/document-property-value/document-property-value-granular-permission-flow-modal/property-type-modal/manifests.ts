import { UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS,
		name: 'Document Property Value Granular User Permission Flow Property Type Modal',
		element: () => import('./property-type-modal.element.js'),
	},
];
