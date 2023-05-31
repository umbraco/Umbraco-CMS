import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.TextArea',
	name: 'Text Area Property Editor UI',
	loader: () => import('./property-editor-ui-textarea.element.js'),
	meta: {
		label: 'Text Area',
		propertyEditorModel: 'Umbraco.TextArea',
		icon: 'umb:edit',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'rows',
					label: 'Number of rows',
					description: 'If empty the textarea is set to autoheight',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'minHeight',
					label: 'Min height (pixels)',
					description: 'Sets the minimum height of the textarea',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'maxHeight',
					label: 'Max height (pixels)',
					description: 'Sets the maximum height of the textarea',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
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
