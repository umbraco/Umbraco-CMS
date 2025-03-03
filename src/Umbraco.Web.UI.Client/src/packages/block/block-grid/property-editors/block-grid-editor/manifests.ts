import { manifest as blockGridSchemaManifest } from './Umbraco.BlockGrid.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import { UmbStandardBlockValueResolver } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		name: 'Block Grid Property Editor UI',
		element: () => import('./property-editor-ui-block-grid.element.js'),
		meta: {
			label: 'Block Grid',
			propertyEditorSchemaAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
			icon: 'icon-layout',
			group: 'richContent',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'blockGroups',
						label: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockTypeGroupConfiguration',
						weight: 1,
					},
					{
						alias: 'useLiveEditing',
						label: 'Live editing mode',
						description: 'Live update content when editing in overlay',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'maxPropertyWidth',
						label: 'Editor width',
						description: 'Optional css overwrite. (example: 1200px or 100%)',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
					{
						alias: 'createLabel',
						label: 'Create Button Label',
						description: 'Override the label text for adding a new block, Example Add Widget',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
					{
						alias: 'gridColumns',
						label: 'Grid Columns',
						description: 'Set the number of columns for the layout.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [
							{ alias: 'min', value: 0 },
							{ alias: 'placeholder', value: '12' },
						],
					},
					{
						alias: 'layoutStylesheet',
						label: 'Layout Stylesheet',
						description: 'Override default stylesheet for backoffice layout.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockGridLayoutStylesheet',
						config: [
							{
								alias: 'singleItemMode',
								value: true,
							},
						],
					},
				],
			},
		},
	},
	blockGridSchemaManifest,
	{
		type: 'propertyValueResolver',
		alias: 'Umb.PropertyValueResolver.BlockGrid',
		name: 'Block Value Resolver',
		api: UmbStandardBlockValueResolver,
		meta: {
			editorAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
		},
	},
];
