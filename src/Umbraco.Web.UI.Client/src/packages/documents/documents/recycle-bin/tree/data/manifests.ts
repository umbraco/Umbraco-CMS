import { UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		name: 'Document Recycle Bin Tree Repository',
		api: () => import('./document-recycle-bin-tree.repository.js'),
	},
];
