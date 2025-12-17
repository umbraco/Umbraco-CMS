import {
	UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS,
} from './constants.js';
import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Single Block',
	alias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	meta: {
		defaultPropertyEditorUiAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS,
		settings: {
			properties: [
				{
					alias: 'blocks',
					label: 'Available Blocks',
					description: 'Define the available blocks.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockSingleTypeConfiguration',
				},
			],
		},
	},
};
