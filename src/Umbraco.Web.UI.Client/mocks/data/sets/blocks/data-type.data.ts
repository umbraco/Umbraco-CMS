import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

// Content type ids are referenced as literals: document types import from this module, so importing
// them back would form a cycle and leave whichever module evaluates second in the temporal dead zone.
const NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID = '9ad6ab46-3eef-4561-bb1c-98ec3026e18a';
const NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID = '9c86f35b-42d9-480f-bc4a-f8475fde6999';

const VARIANT_BLOCK_ELEMENT_TYPE_ID = '086f0418-9ab5-4eef-8073-98e57f0a4cd8';
const VARIANT_HIDDEN_EDITOR_ELEMENT_TYPE_ID = '45bb055d-f580-45f6-95d8-6807a6841b03';

const VISIBILITY_CONTENT_ELEMENT_TYPE_ID = '96eaca75-c166-4845-941f-f54d6570a41b';
const VISIBILITY_EMPTY_CONTENT_ELEMENT_TYPE_ID = '8727fa49-b253-4621-9f21-cce9c6103240';
const VISIBILITY_HIDDEN_EDITOR_ELEMENT_TYPE_ID = 'fe536b69-3383-4492-bed9-bea2333b2c8c';
const VISIBILITY_SETTINGS_ELEMENT_TYPE_ID = '3a65223f-aaa9-42ea-8282-45b9400f7aac';

export const TEXTSTRING_DATA_TYPE_ID = '7df9de83-9e8b-4e75-9fdb-447b6fe7d028';
export const VISIBILITY_BLOCK_LIST_DATA_TYPE_ID = '64db3ae3-83cf-4fd2-ae16-0019cd8c772f';
export const VARIANT_BLOCK_LIST_DATA_TYPE_ID = '33d67562-0871-4a05-b683-2f2f40395d1c';
export const VARIANT_INLINE_BLOCK_LIST_DATA_TYPE_ID = '1f79b6f4-c065-4677-8463-fdf9c83eccd0';
export const NESTED_INNER_BLOCK_LIST_DATA_TYPE_ID = '93f63994-0dc8-4ae7-92c9-60def0668524';
export const NESTED_OUTER_BLOCK_LIST_DATA_TYPE_ID = '95971e5f-32c4-412b-aa01-92685670dbd6';

export const data: Array<UmbMockDataTypeModel> = [
	{
		id: TEXTSTRING_DATA_TYPE_ID,
		parent: null,
		name: 'Textstring',
		editorAlias: 'Umbraco.TextBox',
		editorUiAlias: 'Umb.PropertyEditorUi.TextBox',
		values: [],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
	{
		// Every block has settings, so each one opens a workspace; only the Content view's visibility differs.
		id: VISIBILITY_BLOCK_LIST_DATA_TYPE_ID,
		parent: null,
		name: 'Content Visibility Block List',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: VISIBILITY_CONTENT_ELEMENT_TYPE_ID,
						settingsElementTypeKey: VISIBILITY_SETTINGS_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
					{
						contentElementTypeKey: VISIBILITY_EMPTY_CONTENT_ELEMENT_TYPE_ID,
						settingsElementTypeKey: VISIBILITY_SETTINGS_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
					{
						contentElementTypeKey: VISIBILITY_HIDDEN_EDITOR_ELEMENT_TYPE_ID,
						settingsElementTypeKey: VISIBILITY_SETTINGS_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: true,
					},
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useSingleBlockMode',
				value: false,
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
			{
				alias: 'useInlineEditingAsDefault',
				value: false,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
	{
		id: VARIANT_BLOCK_LIST_DATA_TYPE_ID,
		parent: null,
		name: 'Variant Block List',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
					{
						// The content editor is hidden and there are no settings, so the block has no workspace to open.
						contentElementTypeKey: VARIANT_HIDDEN_EDITOR_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: true,
					},
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useSingleBlockMode',
				value: false,
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
			{
				alias: 'useInlineEditingAsDefault',
				value: false,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
	{
		id: VARIANT_INLINE_BLOCK_LIST_DATA_TYPE_ID,
		parent: null,
		name: 'Variant Inline Block List',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useSingleBlockMode',
				value: false,
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
			{
				alias: 'useInlineEditingAsDefault',
				value: true,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
	{
		id: NESTED_INNER_BLOCK_LIST_DATA_TYPE_ID,
		parent: null,
		name: 'Inner Block List',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useSingleBlockMode',
				value: false,
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
			{
				alias: 'useInlineEditingAsDefault',
				value: true,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
	{
		id: NESTED_OUTER_BLOCK_LIST_DATA_TYPE_ID,
		parent: null,
		name: 'Outer Block List',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useSingleBlockMode',
				value: false,
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
			{
				alias: 'useInlineEditingAsDefault',
				value: true,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
];
