import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

export const TEXTSTRING_DATA_TYPE_ID = 'language-permissions-textstring-data-type-id';
export const BLOCK_LIST_WITH_A_BLOCK_THAT_VARIES_BY_LANGUAGE_DATA_TYPE_ID =
	'language-permissions-block-list-with-a-block-that-varies-by-language-data-type-id';
export const BLOCK_THAT_VARIES_BY_LANGUAGE_ELEMENT_TYPE_ID =
	'language-permissions-block-that-varies-by-language-element-type-id';

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
		id: BLOCK_LIST_WITH_A_BLOCK_THAT_VARIES_BY_LANGUAGE_DATA_TYPE_ID,
		parent: null,
		name: 'Block list with a block that varies by language',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: BLOCK_THAT_VARIES_BY_LANGUAGE_ELEMENT_TYPE_ID,
						label: 'Block (varies by language): {umbValue:sharedText}',
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
				],
			},
			{ alias: 'validationLimit', value: {} },
			{ alias: 'useSingleBlockMode', value: false },
			{ alias: 'useLiveEditing', value: false },
			{ alias: 'useInlineEditingAsDefault', value: false },
		],
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
		noAccess: false,
	},
];
