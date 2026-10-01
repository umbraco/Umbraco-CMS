import { manifest as blockSingleSchemaManifest } from './Umbraco.SingleBlock.js';
import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from './constants.js';
import {
	UmbBlockPropertyValueEntityReferenceResolver,
	UmbStandardBlockValueResolver,
} from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.BlockSingle',
		name: 'Single Block Property Editor UI',
		element: () => import('./property-editor-ui-block-single.element.js'),
		meta: {
			supportsVariantChange: true,
			label: 'Single Block',
			propertyEditorSchemaAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
			icon: 'icon-shape-square',
			group: '#propertyEditorUIGroups_richContent',
			keywords: ['component', 'widget', 'banner', 'hero', 'cta', 'promo', 'cta', 'callout', 'spotlight', 'feature'],
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'useLiveEditing',
						label: 'Live editing mode',
						description: 'Instant updates',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'useInlineEditingAsDefault',
						label: 'Inline editing mode',
						description: 'Expand to edit',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'createModalSize',
						label: '#blockEditor_labelCreateModalSize',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.OverlaySize',
						config: [{ alias: 'defaultOptionLabel', value: 'Auto' }],
					},
					{
						alias: 'maxPropertyWidth',
						label: 'Property Editor width',
						description: 'Example: `800px`',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
						config: [{ alias: 'placeholder', value: '100%' }],
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
	{
		type: 'propertyValueEntityReference',
		alias: 'Umb.PropertyValueEntityReference.BlockSingle',
		name: 'Single Block Entity Reference Resolver',
		api: UmbBlockPropertyValueEntityReferenceResolver,
		forEditorAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	},
	blockSingleSchemaManifest,
];
