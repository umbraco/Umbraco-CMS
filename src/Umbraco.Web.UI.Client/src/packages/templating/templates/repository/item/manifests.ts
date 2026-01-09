import { UmbManagementApiTemplateItemDataCacheInvalidationManager } from './template-item.server.cache-invalidation.manager.js';
import { UmbTemplateItemStore } from './template-item.store.js';

export const UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.TemplateItem';
export const UMB_TEMPLATE_STORE_ALIAS = 'Umb.Store.TemplateItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS,
		name: 'Template Item Repository',
		api: () => import('./template-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_TEMPLATE_STORE_ALIAS,
		name: 'Template Item Store',
		api: UmbTemplateItemStore,
	},
	{
		name: 'Template Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Template',
		type: 'globalContext',
		api: UmbManagementApiTemplateItemDataCacheInvalidationManager,
	},
];
