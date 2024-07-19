import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DocumentBlueprint.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
	name: 'Document Blueprint Folder Repository',
	api: () => import('./document-blueprint-folder.repository.js'),
};

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.DocumentBlueprint.Folder.Rename',
		name: 'Rename Document Blueprint Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.DocumentBlueprint.Folder.Delete',
		name: 'Delete Document Blueprint Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [folderRepository, ...entityActions];
