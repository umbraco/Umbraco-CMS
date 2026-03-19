import { UmbExtensionCollectionDatalistDataSource } from './extension-collection-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.Extension.Type',
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
];
