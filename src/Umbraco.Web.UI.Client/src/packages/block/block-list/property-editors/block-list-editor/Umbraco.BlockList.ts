import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Block List',
	alias: UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
	meta: {
		defaultPropertyEditorUiAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
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
