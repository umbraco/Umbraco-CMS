import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.DatePicker',
	name: 'Date Picker Property Editor UI',
	loader: () => import('./property-editor-ui-date-picker.element'),
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
				{
					alias: 'offsetTime',
					label: 'Offset time',
					description: 'When enabled the time displayed will be offset with the servers timezone, this is useful for scenarios like scheduled publishing when an editor is in a different timezone than the hosted server',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
			defaultData: [
				{
					alias: 'format',
					value: 'YYYY-MM-DD HH:mm:ss',
				},
				{
					alias: 'offsetTime',
					value: false
				}
			],
		},
	},
};
