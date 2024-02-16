import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.MemberGroup.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_MEMBER_GROUP_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Member Group Table Collection View',
	element: () => import('./table/member-group-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.MemberGroup',
		},
	],
};

export const manifests = [tableCollectionView];
