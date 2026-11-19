import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

// Content type ids are referenced as literals: document types import from this module, so importing
// them back would form a cycle and leave whichever module evaluates second in the temporal dead zone.
const NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID = '9ad6ab46-3eef-4561-bb1c-98ec3026e18a';
const NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID = '9c86f35b-42d9-480f-bc4a-f8475fde6999';

export const TEXTSTRING_DATA_TYPE_ID = '7df9de83-9e8b-4e75-9fdb-447b6fe7d028';
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
		noAccess: false,
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
		noAccess: false,
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
		noAccess: false,
	},
];
