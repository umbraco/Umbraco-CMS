import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.RadioButtonList',
	name: 'Radio Button List Property Editor UI',
	loader: () => import('./property-editor-ui-radio-button-list.element.js'),
	meta: {
		label: 'Radio Button List',
		propertyEditorModel: 'Umbraco.RadioButtonList',
		icon: 'umb:target',
		group: 'lists',
		settings: {
			properties: [
				{
					alias: 'items',
					label: 'Add option',
					description: 'Add, remove or sort options for the list.',
					propertyEditorUI: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
