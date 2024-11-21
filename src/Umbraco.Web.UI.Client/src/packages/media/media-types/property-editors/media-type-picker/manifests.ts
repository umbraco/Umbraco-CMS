import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MediaTypePicker',
	name: 'Media Type Picker Property Editor UI',
	element: () => import('./property-editor-ui-media-type-picker.element.js'),
	meta: {
		label: 'Media Type Picker',
		icon: 'icon-media-dashed-line',
		group: 'advanced',
		settings: {
			properties: [
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
