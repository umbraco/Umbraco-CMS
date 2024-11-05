export const UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentBlueprint.Item';
export const UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS = 'Umb.Store.DocumentBlueprint.Item';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
		name: 'Document Blueprint Item Repository',
		api: () => import('./document-blueprint-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS,
		name: 'Document Blueprint Item Store',
		api: () => import('./document-blueprint-item.store.js'),
	},
];
