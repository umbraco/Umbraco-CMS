import { UMB_USER_ENTITY_TYPE } from '@umbraco-cms/backoffice/user';
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
	conditions: [{
		alias: 'Umb.Condition.WorkspaceEntityType',
		match: UMB_USER_ENTITY_TYPE,
	}],
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
	conditions: [{
		alias: 'Umb.Condition.WorkspaceEntityType',
		match: UMB_USER_ENTITY_TYPE,
	}],
};

export const manifests = [tableCollectionView, gridCollectionView];
