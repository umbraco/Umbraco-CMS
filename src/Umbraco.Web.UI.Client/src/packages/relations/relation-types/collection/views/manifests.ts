import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Language.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Language Table Collection View',
	js: () => import('./table/language-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Language',
		},
	],
};

export const manifests = [tableCollectionView];
