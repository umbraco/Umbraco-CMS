import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.StaticImageFilePicker',
	name: 'Static Image File Picker Property Editor UI',
	element: () => import('./property-editor-ui-static-image-file-picker.element.js'),
	meta: {
		label: 'Static Image File Picker',
		icon: 'icon-picture',
		group: '#propertyEditorUIGroups_common',
	},
};
