import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Dropzone.MediaTypePicker',
		name: 'Dropzone Media Type Picker Modal',
		js: () => import('./dropzone-media-type-picker/dropzone-media-type-picker-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
