import { UmbMemberTypeDatalistDataSource } from './member-type-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.MemberType',
		name: 'Member Type Collection Filter',
		meta: {
			label: 'Member Type',
			datalistDataSource: UmbMemberTypeDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
];
