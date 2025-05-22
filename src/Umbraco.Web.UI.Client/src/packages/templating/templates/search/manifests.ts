import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_SEARCH_PROVIDER_ALIAS } from './constants.js';
import { manifests as globalSearchManifests } from './global-search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Template Search Provider',
		alias: UMB_TEMPLATE_SEARCH_PROVIDER_ALIAS,
		type: 'searchProvider',
		api: () => import('./template.search-provider.js'),
		weight: 100,
		meta: {
			label: 'Templates',
		},
	},
	{
		name: 'Template Search Result Item',
		alias: 'Umb.SearchResultItem.Template',
		type: 'searchResultItem',
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE],
	},
	...globalSearchManifests,
];
