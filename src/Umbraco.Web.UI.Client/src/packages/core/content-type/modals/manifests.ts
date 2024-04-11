import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CompositionPicker',
		name: 'ContentType Composition Picker Modal',
		element: () => import('./composition-picker/composition-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertyTypeSettings',
		name: 'Property Type Settings Modal',
		element: () => import('./property-type-settings/property-type-settings-modal.element.js'),
	},
];

export const manifests = modals;
