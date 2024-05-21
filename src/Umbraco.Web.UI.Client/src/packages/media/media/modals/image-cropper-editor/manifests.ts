import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ImageCropPicker',
		name: 'Image Crop Picker Modal',
		js: () => import('./image-cropper-editor-modal.element.js'),
	},
];

export const manifests = [...modals];
