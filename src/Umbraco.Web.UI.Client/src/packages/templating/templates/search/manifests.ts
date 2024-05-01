import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Template Search Provider',
		alias: 'Umb.SearchProvider.Template',
		type: 'searchProvider',
		api: () => import('./template.search-provider.js'),
		weight: 100,
		meta: {
			label: 'Templates',
		},
	},
	{
		name: 'Template Search Result Item ',
		alias: 'Umb.SearchResultItem.Template',
		type: 'searchResultItem',
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE],
	},
];
