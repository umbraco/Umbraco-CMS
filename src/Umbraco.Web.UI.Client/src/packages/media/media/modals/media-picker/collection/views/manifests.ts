import { UMB_MEDIA_PICKER_COLLECTION_ALIAS } from '../index.js';
import { UMB_MEDIA_PICKER_GRID_COLLECTION_VIEW_ALIAS, UMB_MEDIA_PICKER_TABLE_COLLECTION_VIEW_ALIAS } from './index.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

const gridViewManifest: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_MEDIA_PICKER_GRID_COLLECTION_VIEW_ALIAS,
	name: 'Media Picker Grid Collection View',
	element: () => import('./grid/media-picker-grid-collection-view.element.js'),
	weight: 300,
	meta: {
		label: 'Grid',
		icon: 'icon-grid',
		pathName: 'grid',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_PICKER_COLLECTION_ALIAS,
		},
	],
};

const tableViewManifest: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_MEDIA_PICKER_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Media Picker Table Collection View',
	element: () => import('./table/media-picker-table-collection-view.element.js'),
	weight: 200,
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_PICKER_COLLECTION_ALIAS,
		},
	],
};

export const manifests = [gridViewManifest, tableViewManifest];
