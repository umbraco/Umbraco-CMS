export const UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Data Type Collection Repository',
		api: () => import('./data-type-collection.repository.js'),
	},
];
