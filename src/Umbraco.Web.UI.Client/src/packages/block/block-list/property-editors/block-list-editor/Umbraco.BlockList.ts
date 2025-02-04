import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockListTypeConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of blocks',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0 } }],
				},
			],
		},
	},
};
