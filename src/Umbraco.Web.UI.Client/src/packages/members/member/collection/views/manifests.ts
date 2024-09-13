import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const UMB_MEMBER_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Member.Table';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_MEMBER_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Member Table Collection View',
		element: () => import('./table/member-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
];
