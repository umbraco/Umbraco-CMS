import { UMB_SCRIPT_ENTITY_TYPE } from '../../entity.js';
import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../../repository/item/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_SCRIPT_REPOSITORY_ALIAS = 'Umb.Repository.Script.Rename';
export const UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
		name: 'Rename Script Repository',
		api: () => import('./rename-script.repository.js'),
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Rename Script Entity Action',
		kind: 'rename',
		meta: {
			renameRepositoryAlias: UMB_RENAME_SCRIPT_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		},
	},
];
