export const UMB_DOCUMENT_VALIDATION_REPOSITORY_ALIAS = 'Umb.Repository.Document.Validation';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_VALIDATION_REPOSITORY_ALIAS,
		name: 'Document Validation Repository',
		api: () => import('./document-validation.repository.js'),
	},
];
