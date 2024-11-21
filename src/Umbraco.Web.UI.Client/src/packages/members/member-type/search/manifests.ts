import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Member Type Search Provider',
		alias: 'Umb.SearchProvider.MemberType',
		type: 'searchProvider',
		api: () => import('./member-type.search-provider.js'),
		weight: 200,
		meta: {
			label: 'Member Types',
		},
	},
	{
		name: 'Member Type Search Result Item ',
		alias: 'Umb.SearchResultItem.MemberType',
		type: 'searchResultItem',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
	},
];
