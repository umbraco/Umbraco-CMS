import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Block List',
	alias: 'Umbraco.BlockList',
	meta: {
		config: {
			properties: [
				{
					alias: 'blocks',
					label: 'Available Blocks',
					description: 'Define the available blocks.',
					propertyEditorUI: 'Umb.PropertyEditorUI.BlockList.BlockConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of blocks',
					propertyEditorUI: 'Umb.PropertyEditorUI.NumberRange',
				},
			],
		},
	},
};
