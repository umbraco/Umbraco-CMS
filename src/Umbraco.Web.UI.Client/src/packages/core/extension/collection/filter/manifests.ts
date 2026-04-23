import { UMB_EXTENSION_COLLECTION_EXTENSION_TYPE_FACET_FILTER_ALIAS } from './constants.js';
import { UmbExtensionCollectionDatalistDataSource } from './extension-collection-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: UMB_EXTENSION_COLLECTION_EXTENSION_TYPE_FACET_FILTER_ALIAS,
		name: 'Extension Type Collection Filter',
		meta: {
			label: 'Type',
			datalistDataSource: UmbExtensionCollectionDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Extension',
			},
		],
	},
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Umb.Collection.TextFilter.Extension',
		name: 'Extension Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Extension',
			},
		],
	},
];
