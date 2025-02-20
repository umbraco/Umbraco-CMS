import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ImageCropperEditor',
		name: 'Image Cropper Editor Modal',
		js: () => import('./image-cropper-editor-modal.element.js'),
	},
];

export const manifests = [...modals];
