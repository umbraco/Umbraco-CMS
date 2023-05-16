import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.ImageCropper',
	name: 'Image Cropper Property Editor UI',
	loader: () => import('./property-editor-ui-image-cropper.element'),
	meta: {
		label: 'Image Cropper',
		icon: 'umb:colorpicker',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.ImageCropper',
	},
};
