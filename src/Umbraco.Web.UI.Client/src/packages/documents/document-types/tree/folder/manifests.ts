import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type {
	ManifestEntityActions,
	ManifestRepository,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Document Type Folder Repository',
	api: () => import('./document-type-folder.repository.js'),
};

const entityActions: Array<ManifestEntityActions> = [
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
];

export const manifests: Array<ManifestTypes> = [folderRepository, ...entityActions];
