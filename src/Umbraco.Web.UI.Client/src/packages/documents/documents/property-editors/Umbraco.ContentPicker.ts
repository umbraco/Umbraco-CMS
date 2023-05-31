import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Content Picker',
	alias: 'Umbraco.ContentPicker',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.ContentPicker',
		settings: {
			properties: [
				{
					alias: 'startNodeId',
					label: 'Start node',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.TreePicker',
				},
			],
		},
	},
};
