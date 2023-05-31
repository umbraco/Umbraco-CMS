import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.DatePicker',
	name: 'Date Picker Property Editor UI',
	loader: () => import('./property-editor-ui-date-picker.element.js'),
	meta: {
		label: 'Date Picker',
		propertyEditorModel: 'Umbraco.DateTime',
		icon: 'umb:time',
		group: 'pickers',
		config: {
			properties: [
				{
					alias: 'format',
					label: 'Date format',
					description: 'If left empty then the format is YYYY-MM-DD',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
			],
			defaultData: [
				{
					alias: 'format',
					value: 'YYYY-MM-DD HH:mm:ss',
				},
			],
		},
	},
};
