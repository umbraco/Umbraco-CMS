import { UMB_USER_COLLECTION_ALIAS } from '../manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.UserTable',
	name: 'User Table Collection Collection View',
	js: () => import('./table/user-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-box',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_USER_COLLECTION_ALIAS,
		},
	],
};

const gridCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.UserGrid',
	name: 'Media Table Collection View',
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
			match: UMB_USER_COLLECTION_ALIAS,
		},
	],
};

export const manifests = [tableCollectionView, gridCollectionView];
