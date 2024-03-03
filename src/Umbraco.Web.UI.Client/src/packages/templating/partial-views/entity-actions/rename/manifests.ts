import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../../entity.js';
import { UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../../repository/item/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Rename';
export const UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
		name: 'Rename PartialView Repository',
		api: () => import('./rename-partial-view.repository.js'),
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS,
		name: 'Rename PartialView Entity Action',
		kind: 'rename',
		meta: {
			renameRepositoryAlias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		},
	},
];
