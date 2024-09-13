import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.RelationType.Table',
		name: 'Relation Type Table Collection View',
		js: () => import('./table/relation-type-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.RelationType',
			},
		],
	},
];
