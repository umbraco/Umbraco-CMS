import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

// Content type ids are referenced as literals: document types import from this module, so importing
// them back would form a cycle and leave whichever module evaluates second in the temporal dead zone.
const NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID = '9ad6ab46-3eef-4561-bb1c-98ec3026e18a';
const NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID = '9c86f35b-42d9-480f-bc4a-f8475fde6999';

const VARIANT_BLOCK_ELEMENT_TYPE_ID = '086f0418-9ab5-4eef-8073-98e57f0a4cd8';
const VARIANT_HIDDEN_EDITOR_ELEMENT_TYPE_ID = '45bb055d-f580-45f6-95d8-6807a6841b03';

const NESTED_BLOCK_GRID_SECTION_ELEMENT_TYPE_ID = 'c3a5d7f9-1e2b-4c84-a6d0-7b9e3f1a2c52';
const NESTED_BLOCK_GRID_TWO_COLUMNS_ELEMENT_TYPE_ID = 'd8f1a3c5-7b9e-4d20-b4a6-2c5e8f0d1a63';
const NESTED_BLOCK_GRID_TEXT_ELEMENT_TYPE_ID = 'e2b4d6f8-0a1c-4e35-8c7d-9a3b5f1e2d74';

export const TEXTSTRING_DATA_TYPE_ID = '7df9de83-9e8b-4e75-9fdb-447b6fe7d028';
export const VARIANT_BLOCK_LIST_DATA_TYPE_ID = '33d67562-0871-4a05-b683-2f2f40395d1c';
export const VARIANT_INLINE_BLOCK_LIST_DATA_TYPE_ID = '1f79b6f4-c065-4677-8463-fdf9c83eccd0';
export const NESTED_INNER_BLOCK_LIST_DATA_TYPE_ID = '93f63994-0dc8-4ae7-92c9-60def0668524';
export const NESTED_OUTER_BLOCK_LIST_DATA_TYPE_ID = '95971e5f-32c4-412b-aa01-92685670dbd6';
export const NESTED_BLOCK_GRID_DATA_TYPE_ID = 'b7e92f14-3a5c-4d68-8e01-9f4a6c2d5b31';
export const NESTED_BLOCK_GRID_SECTION_AREA_KEY = '5e6f7a8b-9c0d-4e1f-8a3b-4c5d6e7f8a05';
export const NESTED_BLOCK_GRID_LEFT_AREA_KEY = '6f7a8b9c-0d1e-4f2a-9b4c-5d6e7f8a9b06';
export const NESTED_BLOCK_GRID_RIGHT_AREA_KEY = '7a8b9c0d-1e2f-4a3b-8c5d-6e7f8a9b0c07';

const createGridArea = (key: string, alias: string, columnSpan: number) => ({
	key,
	alias,
	columnSpan,
	rowSpan: 1,
	minAllowed: 0,
	specifiedAllowance: [],
});

const createGridBlock = (
	contentElementTypeKey: string,
	options: { allowAtRoot: boolean; allowInAreas: boolean; areas: Array<ReturnType<typeof createGridArea>> },
) => ({
	contentElementTypeKey,
	label: '{=title}',
	columnSpanOptions: [{ columnSpan: 12 }],
	rowMinSpan: 1,
	rowMaxSpan: 1,
	...options,
	editorSize: 'medium',
	inlineEditing: false,
	forceHideContentEditorInOverlay: false,
});

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
	{
		// The layout has no column or row gap, so a Two Columns block is completely covered by the blocks inside it.
		id: NESTED_BLOCK_GRID_DATA_TYPE_ID,
		parent: null,
		name: 'Nested Block Grid',
		editorAlias: 'Umbraco.BlockGrid',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockGrid',
		values: [
			{
				alias: 'blocks',
				value: [
					createGridBlock(NESTED_BLOCK_GRID_SECTION_ELEMENT_TYPE_ID, {
						allowAtRoot: true,
						allowInAreas: false,
						areas: [createGridArea(NESTED_BLOCK_GRID_SECTION_AREA_KEY, 'content', 12)],
					}),
					createGridBlock(NESTED_BLOCK_GRID_TWO_COLUMNS_ELEMENT_TYPE_ID, {
						allowAtRoot: false,
						allowInAreas: true,
						areas: [
							createGridArea(NESTED_BLOCK_GRID_LEFT_AREA_KEY, 'left', 6),
							createGridArea(NESTED_BLOCK_GRID_RIGHT_AREA_KEY, 'right', 6),
						],
					}),
					createGridBlock(NESTED_BLOCK_GRID_TEXT_ELEMENT_TYPE_ID, { allowAtRoot: true, allowInAreas: true, areas: [] }),
				],
			},
			{
				alias: 'validationLimit',
				value: {},
			},
			{
				alias: 'useLiveEditing',
				value: false,
			},
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
	},
];
