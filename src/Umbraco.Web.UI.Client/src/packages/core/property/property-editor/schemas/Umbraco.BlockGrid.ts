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
					description:
						'Define Blocks based on Element Types. Use Groups to help organise their selection. Example Groups: Layout and Content',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGrid.BlockConfiguration',
				},
				{
					alias: 'blockGroups',
					label: 'Block Groups',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGrid.GroupConfiguration',
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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGrid.StylesheetPicker',
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
