import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Color Picker',
	alias: 'Umbraco.ColorPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.ColorPicker',
		settings: {
			properties: [
				{
					alias: 'useLabel',
					label: 'Include labels?',
					description:
						'Stores colors as a Json object containing both the color hex string and label, rather than just the hex string.',
					propertyEditorUI: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'items',
					label: 'Colors',
					description: 'Add, remove or sort colors',
					propertyEditorUI: 'Umb.PropertyEditorUi.ColorPicker',
				},
			],
		},
	},
};
