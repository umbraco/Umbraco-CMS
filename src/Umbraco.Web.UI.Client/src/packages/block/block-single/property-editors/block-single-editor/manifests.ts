import { manifest as blockSingleSchemaManifest } from './Umbraco.SingleBlock.js';
import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from './constants.js';
import { UmbStandardBlockValueResolver } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.BlockSingle',
		name: 'Single Block Property Editor UI',
		element: () => import('./property-editor-ui-block-single.element.js'),
		meta: {
			label: 'Single Block',
			propertyEditorSchemaAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
			icon: 'icon-shape-square',
			group: 'richContent',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'useLiveEditing',
						label: 'Live editing mode',
						description:
							'Live editing in editor overlays for live updated custom views or labels using custom expression.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'useInlineEditingAsDefault',
						label: 'Inline editing mode',
						description: 'Use the inline editor as the default block view.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'maxPropertyWidth',
						label: 'Property editor width',
						description: 'Optional CSS override, example: 800px or 100%',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
			},
		},
	},
	{
		type: 'propertyValueResolver',
		alias: 'Umb.PropertyValueResolver.BlockSingle',
		name: 'Single Block Value Resolver',
		api: UmbStandardBlockValueResolver,
		forEditorAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	},
	blockSingleSchemaManifest,
];
