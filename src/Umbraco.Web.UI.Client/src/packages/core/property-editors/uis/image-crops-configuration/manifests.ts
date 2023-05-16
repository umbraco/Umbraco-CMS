import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.ImageCropsConfiguration',
	name: 'Image Crops Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-image-crops-configuration.element'),
	meta: {
		label: 'Image Crops Configuration',
		icon: 'umb:autofill',
		group: 'common',
		propertyEditorModel: '',
	},
};
