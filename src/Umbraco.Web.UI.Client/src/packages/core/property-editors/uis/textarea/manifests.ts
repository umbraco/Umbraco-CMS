import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TextArea',
	name: 'Text Area Property Editor UI',
	loader: () => import('./property-editor-ui-textarea.element.js'),
	meta: {
		label: 'Text Area',
		propertyEditorAlias: 'Umbraco.TextArea',
		icon: 'umb:edit',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'rows',
					label: 'Number of rows',
					description: 'If empty the textarea is set to autoheight',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'minHeight',
					label: 'Min height (pixels)',
					description: 'Sets the minimum height of the textarea',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'maxHeight',
					label: 'Max height (pixels)',
					description: 'Sets the maximum height of the textarea',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
			],
			defaultData: [
				{
					alias: 'rows',
					value: 10,
				},
			],
		},
	},
};
