import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extensions-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tags',
	alias: 'Umbraco.Tags',
	meta: {
		config: {
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
