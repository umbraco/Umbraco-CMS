import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
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
					propertyEditorUi: 'Umb.PropertyEditorUi.BlockGrid.BlockConfiguration',
				},
				{
					alias: 'blockGroups',
					label: 'Block Groups',
					propertyEditorUi: 'Umb.PropertyEditorUi.BlockGrid.GroupConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					propertyEditorUi: 'Umb.PropertyEditorUi.NumberRange',
				},
				{
					alias: 'gridColumns',
					label: 'Grid Columns',
					description: 'Set the number of columns for the layout. (defaults to 12)',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'layoutStylesheet',
					label: 'Layout Stylesheet',
					description: 'Override default stylesheet for backoffice layout.',
					propertyEditorUi: 'Umb.PropertyEditorUi.BlockGrid.StylesheetPicker',
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
