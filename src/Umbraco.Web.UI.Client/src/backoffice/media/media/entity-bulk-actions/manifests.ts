import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbMediaMoveEntityBulkAction } from './move/move.action';
import { UmbMediaCopyEntityBulkAction } from './copy/copy.action';
import { UmbMediaTrashEntityBulkAction } from './trash/trash.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/extensions-registry';

const entityType = 'media';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 100,
		meta: {
			entityType,
			label: 'Move',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaMoveEntityBulkAction,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Copy',
		name: 'Copy Media Entity Bulk Action',
		weight: 90,
		meta: {
			entityType,
			label: 'Copy',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaCopyEntityBulkAction,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Trash',
		name: 'Trash Media Entity Bulk Action',
		weight: 80,
		meta: {
			entityType,
			label: 'Trash',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaTrashEntityBulkAction,
		},
	},
];

export const manifests = [...entityActions];
