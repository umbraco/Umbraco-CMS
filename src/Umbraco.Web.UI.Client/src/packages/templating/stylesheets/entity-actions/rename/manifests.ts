import { UMB_STYLESHEET_ENTITY_TYPE } from '../../entity.js';
import { UmbRenameStylesheetRepository } from './rename-stylesheet.repository.js';
import { UmbRenameEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Rename';
export const UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
		name: 'Rename Stylesheet Repository',
		api: UmbRenameStylesheetRepository,
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS,
		name: 'Rename Stylesheet Entity Action',
		api: UmbRenameEntityAction,
		weight: 200,
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			repositoryAlias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		},
	},
];
