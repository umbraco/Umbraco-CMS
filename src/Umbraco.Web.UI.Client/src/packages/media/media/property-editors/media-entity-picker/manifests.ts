import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MediaEntityPicker',
	name: 'Media Entity Picker Property Editor UI',
	element: () => import('./property-editor-ui-media-entity-picker.element.js'),
	meta: {
		label: 'Media Entity Picker',
		icon: 'icon-picture',
		group: 'pickers',
		supportsReadOnly: true,
	},
};
