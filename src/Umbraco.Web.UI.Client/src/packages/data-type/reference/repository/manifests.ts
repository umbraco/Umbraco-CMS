export const UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Reference';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS,
		name: 'Data Type Reference Repository',
		api: () => import('./data-type-reference.repository.js'),
	},
];
