import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Member Search Provider',
		alias: 'Umb.SearchProvider.Member',
		type: 'searchProvider',
		api: () => import('./member.search-provider.js'),
		weight: 300,
		meta: {
			label: 'Members',
		},
	},
	{
		name: 'Member Search Result Item ',
		alias: 'Umb.SearchResultItem.Member',
		type: 'searchResultItem',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
	},
];
