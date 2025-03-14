import { manifests as propertyTypeModalManifests } from './property-type-modal/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentValueGranularUserPermissionFlow',
		name: 'Document Value Granular User Permission Flow Modal',
		element: () => import('./document-value-granular-permission-flow-modal.element.js'),
	},
	...propertyTypeModalManifests,
];
