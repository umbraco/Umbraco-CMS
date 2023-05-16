import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.CheckboxList',
	name: 'Checkbox List Property Editor UI',
	loader: () => import('./property-editor-ui-checkbox-list.element'),
	meta: {
		label: 'Checkbox List',
		propertyEditorModel: 'Umbraco.CheckboxList',
		icon: 'umb:bulleted-list',
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
