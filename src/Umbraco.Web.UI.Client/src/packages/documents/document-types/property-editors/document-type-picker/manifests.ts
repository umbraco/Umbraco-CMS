import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentTypePicker',
	name: 'Document Type Picker Property Editor UI',
	element: () => import('./property-editor-ui-document-type-picker.element.js'),
	meta: {
		label: 'Document Type Picker',
		icon: 'icon-document-dashed-line',
		group: 'advanced',
		settings: {
			properties: [
				{
					alias: 'onlyPickElementTypes',
					label: 'Only Element Types',
					description: 'Limit to only pick Element Types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
