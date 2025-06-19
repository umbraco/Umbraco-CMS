export const UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.RelationType.Detail';
export const UMB_RELATION_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.RelationType.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Relation Type Detail Repository',
		api: () => import('./relation-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_RELATION_TYPE_DETAIL_STORE_ALIAS,
		name: 'Relation Type Detail Store',
		api: () => import('./relation-type-detail.store.js'),
	},
];
