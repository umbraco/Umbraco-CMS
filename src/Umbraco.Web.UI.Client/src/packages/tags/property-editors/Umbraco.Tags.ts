import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tags',
	alias: 'Umbraco.Tags',
	meta: {
		settings: {
			properties: [
				{
					alias: 'startNodeId',
					label: 'Start node',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Tags',
				},
			],
		},
	},
};
