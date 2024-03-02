import { UMB_STYLESHEET_ENTITY_TYPE } from '../../entity.js';
import { UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Rename';
export const UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
		name: 'Rename Stylesheet Repository',
		api: () => import('./rename-stylesheet.repository.js'),
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_STYLESHEET_ENTITY_ACTION_ALIAS,
		name: 'Rename Stylesheet Entity Action',
		kind: 'rename',
		meta: {
			renameRepositoryAlias: UMB_RENAME_STYLESHEET_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		},
	},
];
