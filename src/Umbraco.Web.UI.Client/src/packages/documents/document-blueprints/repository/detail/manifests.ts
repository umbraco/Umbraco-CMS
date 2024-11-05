export const UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.DocumentBlueprint.Detail';
export const UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS = 'Umb.Store.DocumentBlueprint.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
		name: 'Document Blueprint Detail Repository',
		api: () => import('./document-blueprint-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS,
		name: 'Document Blueprint Detail Store',
		api: () => import('./document-blueprint-detail.store.js'),
	},
];
