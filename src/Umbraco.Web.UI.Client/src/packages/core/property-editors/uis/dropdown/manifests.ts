import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Dropdown',
	name: 'Dropdown Property Editor UI',
	loader: () => import('./property-editor-ui-dropdown.element.js'),
	meta: {
		label: 'Dropdown',
		propertyEditorModel: 'Umbraco.Dropdown',
		icon: 'umb:time',
		group: 'pickers',
		settings: {
			properties: [
				{
					alias: 'multiple',
					label: 'Enable multiple choice',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'items',
					label: 'Add options',
					propertyEditorUi: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
