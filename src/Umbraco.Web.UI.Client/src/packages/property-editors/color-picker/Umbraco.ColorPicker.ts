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
					label: '#colorPickerConfigurations_showLabelTitle',
					description: '{umbLocalize: colorPickerConfigurations_showLabelDescription}',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'items',
					label: '#colorPickerConfigurations_colorsTitle',
					description: '{umbLocalize: colorPickerConfigurations_colorsDescription}',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ColorSwatchesEditor',
				},
			],
		},
	},
};
