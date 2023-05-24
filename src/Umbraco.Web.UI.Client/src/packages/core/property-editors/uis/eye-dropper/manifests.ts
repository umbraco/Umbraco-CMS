import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.EyeDropper',
	name: 'Eye Dropper Color Picker Property Editor UI',
	loader: () => import('./property-editor-ui-eye-dropper.element.js'),
	meta: {
		label: 'Eye Dropper Color Picker',
		icon: 'umb:colorpicker',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.ColorPicker.EyeDropper',
		config: {
			properties: [
				{
					alias: 'showAlpha',
					label: 'Show alpha',
					description: 'Allow alpha transparency selection.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'showPalette',
					label: 'Show palette',
					description: 'Show a palette next to the color picker.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};
