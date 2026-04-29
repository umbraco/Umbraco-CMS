import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

export const BLM_BL_INVARIANT_ET_DT_ID = 'blm-bl-invariant-et-dt';
export const BLM_BL_VARIANT_ET_INVARIANT_TEXT_DT_ID = 'blm-bl-variant-et-invariant-text-dt';
export const BLM_BL_VARIANT_ET_VARIANT_TEXT_DT_ID = 'blm-bl-variant-et-variant-text-dt';

export const data: Array<UmbMockDataTypeModel> = [
	{
		id: 'variant-documents-textstring-data-type-id',
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
		id: BLM_BL_INVARIANT_ET_DT_ID,
		parent: null,
		name: 'Block List (Invariant Elements)',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
		values: [
			{
				alias: 'blocks',
				value: [
					{
						label: 'Invariant Element',
						contentElementTypeKey: 'blm-et-invariant',
					},
				],
			},
		],
	},
	{
		id: BLM_BL_VARIANT_ET_INVARIANT_TEXT_DT_ID,
		parent: null,
		name: 'Block List (Variant Elements, Invariant Text)',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
		values: [
			{
				alias: 'blocks',
				value: [
					{
						label: 'Variant Element (Invariant Text)',
						contentElementTypeKey: 'blm-et-variant-invariant-text',
					},
				],
			},
		],
	},
	{
		id: BLM_BL_VARIANT_ET_VARIANT_TEXT_DT_ID,
		parent: null,
		name: 'Block List (Variant Elements, Variant Text)',
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		hasChildren: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
		values: [
			{
				alias: 'blocks',
				value: [
					{
						label: 'Variant Element (Variant Text)',
						contentElementTypeKey: 'blm-et-variant-variant-text',
					},
				],
			},
		],
	},
];
