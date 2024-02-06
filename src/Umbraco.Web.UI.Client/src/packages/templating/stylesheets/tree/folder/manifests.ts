import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbStylesheetFolderRepository } from './stylesheet-folder.repository.js';
import { UmbDeleteFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
	name: 'Stylesheet Folder Repository',
	api: UmbStylesheetFolderRepository,
};

export const UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Folder.Delete';

const entityActions = [
	{
		type: 'entityAction',
		alias: UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Stylesheet folder Entity Action',
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
