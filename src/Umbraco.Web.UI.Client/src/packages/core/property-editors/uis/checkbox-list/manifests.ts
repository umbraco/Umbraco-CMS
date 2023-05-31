import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.CheckboxList',
	name: 'Checkbox List Property Editor UI',
	loader: () => import('./property-editor-ui-checkbox-list.element.js'),
	meta: {
		label: 'Checkbox List',
		propertyEditorModel: 'Umbraco.CheckboxList',
		icon: 'umb:bulleted-list',
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
