export const UMB_TAG_REPOSITORY_ALIAS = 'Umb.Repository.Tags';
export const UMB_TAG_STORE_ALIAS = 'Umb.Store.Tags';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TAG_REPOSITORY_ALIAS,
		name: 'Tags Repository',
		api: () => import('./tag.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_TAG_STORE_ALIAS,
		name: 'Tags Store',
		api: () => import('./tag.store.js'),
	},
];
