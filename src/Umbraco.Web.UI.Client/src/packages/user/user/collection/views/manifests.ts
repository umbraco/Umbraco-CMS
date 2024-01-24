import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_COLLECTION_VIEW_USER_TABLE = 'Umb.CollectionView.User.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_COLLECTION_VIEW_USER_TABLE,
	name: 'User Table Collection View',
	js: () => import('./table/user-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const UMB_COLLECTION_VIEW_USER_GRID = 'Umb.CollectionView.User.Grid';

const gridCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_COLLECTION_VIEW_USER_GRID,
	name: 'User Table Collection View',
	js: () => import('./grid/user-grid-collection-view.element.js'),
	weight: 200,
	meta: {
		label: 'Grid',
		icon: 'icon-grid',
		pathName: 'grid',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const manifests = [tableCollectionView, gridCollectionView];
