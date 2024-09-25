import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_FOLDER_STORE_ALIAS } from './constants.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Document Type Folder Repository',
		api: () => import('./document-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_TYPE_FOLDER_STORE_ALIAS,
		name: 'Document Type Store',
		api: () => import('./document-type-folder.store.js'),
	},
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.DocumentType.Folder.Rename',
		name: 'Rename Document Type Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.DocumentType.Folder.Delete',
		name: 'Delete Document Type Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	...workspaceManifests,
];
