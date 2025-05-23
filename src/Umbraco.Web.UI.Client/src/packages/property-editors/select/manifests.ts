import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Select',
	name: 'Select Property Editor UI',
	element: () => import('./property-editor-ui-select.element.js'),
	meta: {
		label: 'Select',
		icon: 'icon-list',
		group: 'pickers',
		settings: {
			properties: [
				{
					alias: 'items',
					label: 'Add options',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
