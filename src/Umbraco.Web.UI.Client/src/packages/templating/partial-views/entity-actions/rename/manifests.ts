import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../../entity.js';
import { UmbRenamePartialViewRepository } from './rename-partial-view.repository.js';
import { UmbRenameEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Rename';
export const UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Rename';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'repository',
		alias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
		name: 'Rename PartialView Repository',
		api: UmbRenamePartialViewRepository,
	},
	{
		type: 'entityAction',
		alias: UMB_RENAME_PARTIAL_VIEW_ENTITY_ACTION_ALIAS,
		name: 'Rename PartialView Entity Action',
		api: UmbRenameEntityAction,
		weight: 200,
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			repositoryAlias: UMB_RENAME_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		},
	},
];
