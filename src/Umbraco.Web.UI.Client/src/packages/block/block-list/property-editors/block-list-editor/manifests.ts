import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import { manifest as propertyEditorSchema } from './Umbraco.BlockList.js';
import { UmbStandardBlockValueResolver } from '@umbraco-cms/backoffice/block';
import { UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

const propertyEditorUi: UmbExtensionManifest = {
	type: 'propertyEditorUi',
	alias: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
	name: 'Block List Property Editor UI',
	element: () => import('./property-editor-ui-block-list.element.js'),
	meta: {
		label: 'Block List',
		propertyEditorSchemaAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
		icon: 'icon-thumbnail-list',
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
				{
					alias: 'useSingleBlockMode',
					label: 'Single block mode',
					description: '_Deprecated: Use the Property Editor "Single Block" instead._',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const propertyValueResolver: UmbExtensionManifest = {
	type: 'propertyValueResolver',
	alias: 'Umb.PropertyValueResolver.BlockList',
	name: 'Block List Value Resolver',
	api: UmbStandardBlockValueResolver,
	forEditorAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
};

const sortModePropertyContext: UmbExtensionManifest = {
	type: 'propertyContext',
	kind: 'sortMode',
	alias: 'Umb.PropertyContext.BlockList.SortMode',
	name: 'Block List Sort Mode Property Context',
	forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
};

const sortModePropertyAction: UmbExtensionManifest = {
	type: 'propertyAction',
	kind: 'sortMode',
	alias: 'Umb.PropertyAction.BlockList.SortMode',
	name: 'Block List Sort Mode Property Action',
	forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
	conditions: [
		{
			alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [
	propertyEditorSchema,
	propertyEditorUi,
	propertyValueResolver,
	sortModePropertyContext,
	sortModePropertyAction,
];
