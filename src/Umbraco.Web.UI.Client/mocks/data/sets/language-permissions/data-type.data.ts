import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

export const TEXTSTRING_DATA_TYPE_ID = 'language-permissions-textstring-data-type-id';

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
];
