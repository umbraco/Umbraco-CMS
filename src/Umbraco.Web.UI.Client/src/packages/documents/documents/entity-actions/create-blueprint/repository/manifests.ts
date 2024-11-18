export const UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS = 'Umb.Repository.Document.CreateBlueprint';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS,
		name: 'Document Create Blueprint Repository',
		api: () => import('./document-create-blueprint.repository.js'),
	},
];
