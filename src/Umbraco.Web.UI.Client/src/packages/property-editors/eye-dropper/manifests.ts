import { manifest as schemaManifest } from './Umbraco.ColorPicker.EyeDropper.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.EyeDropper',
		name: 'Eye Dropper Color Picker Property Editor UI',
		element: () => import('./property-editor-ui-eye-dropper.element.js'),
		meta: {
			label: 'Eye Dropper Color Picker',
			icon: 'icon-colorpicker',
			group: 'pickers',
			propertyEditorSchemaAlias: 'Umbraco.ColorPicker.EyeDropper',
			settings: {
				properties: [
					{
						alias: 'showAlpha',
						label: 'Show alpha',
						description: 'Allow alpha transparency selection.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'showPalette',
						label: 'Show palette',
						description: 'Show a palette next to the color picker.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
				],
			},
		},
	},
	schemaManifest,
];
