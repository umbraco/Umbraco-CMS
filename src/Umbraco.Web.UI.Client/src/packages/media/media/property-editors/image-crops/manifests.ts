import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ImageCropsConfiguration',
	name: 'Image Crops Property Editor UI',
	element: () => import('./property-editor-ui-image-crops.element.js'),
	meta: {
		label: 'Image Crops Configuration',
		icon: 'icon-autofill',
		group: 'common',
	},
};
