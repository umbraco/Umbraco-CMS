import { UMB_USER_ENTITY_TYPE } from '@umbraco-cms/backoffice/user';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.UserTable',
	name: 'User Table Collection Collection View',
	loader: () => import('./table/user-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'umb:box',
		pathName: 'table',
	},
	conditions: {
		entityType: UMB_USER_ENTITY_TYPE,
	},
};

const gridCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: 'Umb.CollectionView.UserGrid',
	name: 'Media Table Collection View',
	loader: () => import('./grid/user-grid-collection-view.element.js'),
	weight: 200,
	meta: {
		label: 'Grid',
		icon: 'umb:grid',
		pathName: 'grid',
	},
	conditions: {
		entityType: UMB_USER_ENTITY_TYPE,
	},
};

export const manifests = [tableCollectionView, gridCollectionView];
