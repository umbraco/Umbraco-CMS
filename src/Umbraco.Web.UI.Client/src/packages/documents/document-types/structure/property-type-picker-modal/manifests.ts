import { UMB_DOCUMENT_TYPE_PROPERTY_TYPE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_TYPE_PROPERTY_TYPE_PICKER_MODAL_ALIAS,
		name: 'Document Type Property Type Picker Modal',
		element: () => import('./document-type-property-type-picker-modal.element.js'),
	},
];
