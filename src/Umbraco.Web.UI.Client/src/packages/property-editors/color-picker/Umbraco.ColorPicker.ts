import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'items',
					label: 'Colors',
					description: 'Add, remove or sort colors',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ColorSwatchesEditor',
				},
			],
		},
	},
};
