import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Dropdown',
	name: 'Dropdown Property Editor UI',
	loader: () => import('./property-editor-ui-dropdown.element'),
	meta: {
		label: 'Dropdown',
		propertyEditorModel: 'Umbraco.Dropdown',
		icon: 'umb:time',
		group: 'pickers',
		config: {
			properties: [
				{
					alias: 'multiple',
					label: 'Enable multiple choice',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'items',
					label: 'Add options',
					propertyEditorUI: 'Umb.PropertyEditorUI.MultipleTextString',
				},
			],
		},
	},
};
