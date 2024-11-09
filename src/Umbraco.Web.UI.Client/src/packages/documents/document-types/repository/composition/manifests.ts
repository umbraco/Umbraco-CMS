export const UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Composition';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		name: 'Document Type Composition Repository',
		api: () => import('./document-type-composition.repository.js'),
	},
];
