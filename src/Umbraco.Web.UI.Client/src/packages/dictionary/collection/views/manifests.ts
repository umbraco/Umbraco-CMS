import { UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Dictionary Table Collection View',
	element: () => import('./table/dictionary-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Dictionary',
		},
	],
};

export const manifests: Array<ManifestTypes> = [tableCollectionView];
