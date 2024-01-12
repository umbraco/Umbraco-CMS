import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Block Grid',
	alias: 'Umbraco.BlockGrid',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGrid',
		settings: {
			properties: [
				{
					alias: 'blocks',
					label: 'Blocks',
					description: 'Define Blocks based on Element Types.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGridTypeConfiguration',
				},
				{
					alias: 'blockGroups',
					label: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockTypeGroupConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
				},
				{
					alias: 'gridColumns',
					label: 'Grid Columns',
					description: 'Set the number of columns for the layout. (defaults to 12)',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'layoutStylesheet',
					label: 'Layout Stylesheet',
					description: 'Override default stylesheet for backoffice layout.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGridLayoutStylesheet',
				},
			],
			defaultData: [
				{
					alias: 'gridColumns',
					value: 12,
				},
			],
		},
	},
};
