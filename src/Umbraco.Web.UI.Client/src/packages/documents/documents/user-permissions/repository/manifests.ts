export const UMB_DOCUMENT_PERMISSION_REPOSITORY_ALIAS = 'Umb.Repository.Document.Permission';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_PERMISSION_REPOSITORY_ALIAS,
		name: 'Document Permission Repository',
		api: () => import('./document-permission.repository.js'),
	},
];
