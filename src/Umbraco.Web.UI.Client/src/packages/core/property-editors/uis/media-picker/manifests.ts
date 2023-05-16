import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.MediaPicker',
	name: 'Markdown Editor Property Editor UI',
	loader: () => import('./property-editor-ui-media-picker.element'),
	meta: {
		label: 'Media Picker',
		propertyEditorModel: 'Umbraco.MediaPicker3',
		icon: 'umb:picture',
		group: 'pickers',
	},
};
