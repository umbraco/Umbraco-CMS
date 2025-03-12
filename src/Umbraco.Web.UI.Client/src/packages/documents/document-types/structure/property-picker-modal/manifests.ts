import { UMB_DOCUMENT_TYPE_PROPERTY_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_TYPE_PROPERTY_PICKER_MODAL_ALIAS,
		name: 'Document Type Property Picker Modal',
		element: () => import('./document-type-property-picker-modal.element.js'),
	},
];
