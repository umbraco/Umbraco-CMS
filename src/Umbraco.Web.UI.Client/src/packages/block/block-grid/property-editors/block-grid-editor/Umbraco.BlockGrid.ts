import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Block Grid',
	alias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
	meta: {
		defaultPropertyEditorUiAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
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
