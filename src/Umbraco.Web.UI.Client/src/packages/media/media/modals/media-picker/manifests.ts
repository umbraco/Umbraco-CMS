import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaPicker',
		name: 'Media Picker Modal',
		js: () => import('./media-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
