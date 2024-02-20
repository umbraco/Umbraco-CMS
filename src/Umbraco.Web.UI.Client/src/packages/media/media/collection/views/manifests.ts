import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS, UMB_MEDIA_TABLE_COLLECTION_VIEW_ALIAS } from './index.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

const gridViewManifest: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS,
	name: 'Media Grid Collection View',
	element: () => import('./grid/media-grid-collection-view.element.js'),
	weight: 300,
	meta: {
		label: 'Grid',
		icon: 'icon-grid',
		pathName: 'grid',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Media',
		},
	],
};

const tableViewManifest: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_MEDIA_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Media Table Collection View',
	element: () => import('./table/media-table-collection-view.element.js'),
	weight: 200,
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Media',
		},
	],
};

export const manifests = [gridViewManifest, tableViewManifest];
