import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Block Grid',
	alias: 'Umbraco.BlockGrid',
	meta: {
		config: {
			properties: [
				{
					alias: 'blocks',
					label: 'Blocks',
					description:
						'Define Blocks based on Element Types. Use Groups to help organise their selection. Example Groups: Layout and Content',
					propertyEditorUI: 'Umb.PropertyEditorUI.BlockGrid.BlockConfiguration',
				},
				{
					alias: 'blockGroups',
					label: 'Block Groups',
					propertyEditorUI: 'Umb.PropertyEditorUI.BlockGrid.GroupConfiguration',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					propertyEditorUI: 'Umb.PropertyEditorUI.NumberRange',
				},
				{
					alias: 'gridColumns',
					label: 'Grid Columns',
					description: 'Set the number of columns for the layout. (defaults to 12)',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'layoutStylesheet',
					label: 'Layout Stylesheet',
					description: 'Override default stylesheet for backoffice layout.',
					propertyEditorUI: 'Umb.PropertyEditorUI.BlockGrid.StylesheetPicker',
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
