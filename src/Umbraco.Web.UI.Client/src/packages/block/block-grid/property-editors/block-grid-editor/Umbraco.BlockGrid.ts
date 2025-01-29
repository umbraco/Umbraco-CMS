import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

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
					alias: 'validationLimit',
					label: 'Amount',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
					weight: 100,
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
