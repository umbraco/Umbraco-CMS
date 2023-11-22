import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.User.Table',
	name: 'User Table Collection View',
	js: () => import('./table/user-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-box',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

const gridCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.User.Grid',
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
