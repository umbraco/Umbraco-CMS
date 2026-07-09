import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../../entity.js';
import { UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../../repository/item/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Rename';
export const UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Rename';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
		name: 'Rename PartialView Repository',
		api: () => import('./rename-partial-view.repository.js'),
	},
	{
		type: 'entityAction',
		kind: 'renameServerFile',
		alias: UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS,
		name: 'Rename Partial View Entity Action',
		forEntityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		meta: {
			renameRepositoryAlias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
