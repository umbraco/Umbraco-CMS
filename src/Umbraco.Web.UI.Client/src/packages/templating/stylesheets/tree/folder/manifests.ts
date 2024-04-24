import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
	name: 'Stylesheet Folder Repository',
	api: () => import('./stylesheet-folder.repository.js'),
};

export const UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Folder.Delete';

const entityActions = [
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Stylesheet folder Entity Action',
		forEntityTypes: [UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [folderRepository, ...entityActions];
