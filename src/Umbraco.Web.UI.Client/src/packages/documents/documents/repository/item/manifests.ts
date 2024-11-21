export const UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentItem';
export const UMB_DOCUMENT_STORE_ALIAS = 'Umb.Store.DocumentItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
		name: 'Document Item Repository',
		api: () => import('./document-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_STORE_ALIAS,
		name: 'Document Item Store',
		api: () => import('./document-item.store.js'),
	},
];
