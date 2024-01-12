import { UMB_SCRIPT_ENTITY_TYPE } from '../../entity.js';
import { UmbRenameScriptRepository } from './rename-script.repository.js';
import { UmbRenameEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_SCRIPT_REPOSITORY_ALIAS = 'Umb.Repository.Script.Rename';
export const UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
		name: 'Rename Script Repository',
		api: UmbRenameScriptRepository,
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Rename Script Entity Action',
		api: UmbRenameEntityAction,
		weight: 200,
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			repositoryAlias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		},
	},
];
