import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.RadioButtonList',
	name: 'Radio Button List Property Editor UI',
	loader: () => import('./property-editor-ui-radio-button-list.element.js'),
	meta: {
		label: 'Radio Button List',
		propertyEditorModel: 'Umbraco.RadioButtonList',
		icon: 'umb:target',
		group: 'lists',
		config: {
			properties: [
				{
					alias: 'items',
					label: 'Add option',
					description: 'Add, remove or sort options for the list.',
					propertyEditorUI: 'Umb.PropertyEditorUI.MultipleTextString',
				},
			],
		},
	},
};
