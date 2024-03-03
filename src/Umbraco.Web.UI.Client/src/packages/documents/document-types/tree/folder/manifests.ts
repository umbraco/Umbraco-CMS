import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentTypeFolderRepository } from './document-type-folder.repository.js';
import { UmbDeleteFolderEntityAction, UmbFolderUpdateEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Document Type Folder Repository',
	api: UmbDocumentTypeFolderRepository,
};

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.RenameFolder',
		name: 'Rename Document Type Folder Entity Action',
		weight: 800,
		api: UmbFolderUpdateEntityAction,
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-edit',
			label: 'Rename Folder...',
			repositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.DeleteFolder',
		name: 'Delete Document Type Folder Entity Action',
		weight: 700,
		api: UmbDeleteFolderEntityAction,
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-trash',
			label: 'Delete Folder...',
			repositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
