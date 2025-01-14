import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Member Search Provider',
		alias: UMB_MEMBER_SEARCH_PROVIDER_ALIAS,
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
