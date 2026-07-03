import { UMB_STYLESHEET_ENTITY_TYPE } from '../../entity.js';
import { UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Rename';
export const UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Rename';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
		name: 'Rename Stylesheet Repository',
		api: () => import('./rename-stylesheet.repository.js'),
	},
	{
		type: 'entityAction',
		kind: 'renameServerFile',
		alias: UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS,
		name: 'Rename Stylesheet Entity Action',
		forEntityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		meta: {
			renameRepositoryAlias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
