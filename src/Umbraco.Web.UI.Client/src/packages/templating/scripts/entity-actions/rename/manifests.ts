import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../../entity.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const UMB_RENAME_SCRIPT_REPOSITORY_ALIAS = 'Umb.Repository.Script.Rename';
export const UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Rename';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
		name: 'Rename Script Repository',
		api: () => import('./rename-script.repository.js'),
	},
	{
		type: 'entityAction',
		kind: 'renameServerFile',
		alias: UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Rename Script Entity Action',
		forEntityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		meta: {
			renameRepositoryAlias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
