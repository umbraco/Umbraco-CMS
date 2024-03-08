import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CompositionPicker',
		name: 'ContentType Composition Picker Modal',
		js: () => import('./composition-picker/composition-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertySettings',
		name: 'Property Settings Modal',
		js: () => import('.//property-settings/property-settings-modal.element.js'),
	},
];

export const manifests = [...modals];
