import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaPicker',
		name: 'Media Picker Modal',
		loader: () => import('./media-picker/media-picker-modal.element'),
	},
];

export const manifests = [...modals];
