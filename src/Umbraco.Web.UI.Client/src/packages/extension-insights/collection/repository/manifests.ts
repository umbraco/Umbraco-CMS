export const UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.ExtensionCollection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
		name: 'Extension Collection Repository',
		api: () => import('./extension-collection.repository.js'),
	},
];
