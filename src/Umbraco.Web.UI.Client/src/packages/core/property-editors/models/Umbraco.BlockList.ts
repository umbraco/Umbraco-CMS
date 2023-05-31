import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Block List',
	alias: 'Umbraco.BlockList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		settings: {
			properties: [
				{
					alias: 'blocks',
					label: 'Available Blocks',
					description: 'Define the available blocks.',
					propertyEditorUI: 'Umb.PropertyEditorUi.BlockList.BlockConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of blocks',
					propertyEditorUI: 'Umb.PropertyEditorUi.NumberRange',
				},
			],
		},
	},
};
