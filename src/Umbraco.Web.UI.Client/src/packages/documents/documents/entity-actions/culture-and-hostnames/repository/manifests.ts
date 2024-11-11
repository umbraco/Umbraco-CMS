export const UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS = 'Umb.Repository.Document.CultureAndHostnames';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS,
		name: 'Document Culture And Hostnames Repository',
		api: () => import('./culture-and-hostnames.repository.js'),
	},
];
