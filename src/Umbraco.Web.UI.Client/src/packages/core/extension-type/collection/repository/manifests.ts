export const UMB_EXTENSION_TYPE_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.ExtensionTypeCollection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXTENSION_TYPE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Extension Type Collection Repository',
		api: () => import('./extension-type-collection.repository.js'),
	},
];
