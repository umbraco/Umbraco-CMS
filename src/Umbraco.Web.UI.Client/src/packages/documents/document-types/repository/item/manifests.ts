export const UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Item';
export const UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.DocumentType.Item';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Document Type Item Repository',
		api: () => import('./document-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS,
		name: 'Document Type Item Store',
		api: () => import('./document-type-item.store.js'),
	},
];
