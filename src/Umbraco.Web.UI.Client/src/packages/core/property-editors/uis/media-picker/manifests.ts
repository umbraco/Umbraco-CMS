import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MediaPicker',
	name: 'Markdown Editor Property Editor UI',
	loader: () => import('./property-editor-ui-media-picker.element.js'),
	meta: {
		label: 'Media Picker',
		propertyEditorAlias: 'Umbraco.MediaPicker3',
		icon: 'umb:picture',
		group: 'pickers',
	},
};
