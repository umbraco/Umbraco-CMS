import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';

export const COLLECTION_DATA_TYPE_ID = 'documents-collection-data-type-id';

export const data: Array<UmbMockDataTypeModel> = [
	{
		id: COLLECTION_DATA_TYPE_ID,
		parent: null,
		name: 'Collection',
		editorAlias: 'Umbraco.ListView',
		editorUiAlias: 'Umb.PropertyEditorUi.Collection',
		hasChildren: false,
		isFolder: false,
		isDeletable: false,
		canIgnoreStartNodes: false,
		flags: [],
		values: [
			{ alias: 'pageSize', value: 10 },
			{ alias: 'orderBy', value: 'updateDate' },
			{ alias: 'orderDirection', value: 'desc' },
			{
				alias: 'includeProperties',
				value: [
					{ alias: 'sortOrder', header: 'Sort order', isSystem: true, nameTemplate: '' },
					{ alias: 'updateDate', header: 'Last edited', isSystem: true },
					{ alias: 'creator', header: 'Created by', isSystem: true },
				],
			},
			{
				alias: 'layouts',
				value: [
					{
						icon: 'icon-list',
						name: 'Document Table Collection View',
						collectionView: 'Umb.CollectionView.Document.Table',
					},
					{
						icon: 'icon-grid',
						name: 'Document Grid Collection View',
						collectionView: 'Umb.CollectionView.Document.Grid',
					},
				],
			},
			{ alias: 'icon', value: 'icon-layers' },
			{ alias: 'tabName', value: 'Children' },
			{ alias: 'showContentFirst', value: true },
		],
		noAccess: false,
	},
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
		noAccess: false,
	},
];
