import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.TextArea',
	name: 'Text Area Property Editor UI',
	loader: () => import('./property-editor-ui-textarea.element'),
	meta: {
		label: 'Text Area',
		propertyEditorModel: 'Umbraco.TextArea',
		icon: 'umb:edit',
		group: 'common',
		config: {
			properties: [
				{
					alias: 'rows',
					label: 'Number of rows',
					description: 'If empty - 10 rows would be set as the default value',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
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
