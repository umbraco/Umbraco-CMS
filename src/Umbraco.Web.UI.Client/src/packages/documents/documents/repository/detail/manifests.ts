export const UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Document.Detail';
export const UMB_DOCUMENT_DETAIL_STORE_ALIAS = 'Umb.Store.Document.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		name: 'Document Detail Repository',
		api: () => import('./document-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_DETAIL_STORE_ALIAS,
		name: 'Document Detail Store',
		api: () => import('./document-detail.store.js'),
	},
];
