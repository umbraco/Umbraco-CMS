import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockList.BlockConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of blocks',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
				},
			],
		},
	},
};
