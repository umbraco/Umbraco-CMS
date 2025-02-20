export const UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Template.Detail';
export const UMB_TEMPLATE_DETAIL_STORE_ALIAS = 'Umb.Store.Template.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
		name: 'Template Detail Repository',
		api: () => import('./template-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_TEMPLATE_DETAIL_STORE_ALIAS,
		name: 'Template Detail Store',
		api: () => import('./template-detail.store.js'),
	},
];
