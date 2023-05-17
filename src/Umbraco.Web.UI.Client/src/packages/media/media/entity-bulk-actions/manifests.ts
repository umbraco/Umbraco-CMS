import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbMediaMoveEntityBulkAction } from './move/move.action';
import { UmbMediaCopyEntityBulkAction } from './copy/copy.action';
import { UmbMediaTrashEntityBulkAction } from './trash/trash.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'media';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 100,
		meta: {
			label: 'Move',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaMoveEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Copy',
		name: 'Copy Media Entity Bulk Action',
		weight: 90,
		meta: {
			label: 'Copy',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaCopyEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Trash',
		name: 'Trash Media Entity Bulk Action',
		weight: 80,
		meta: {
			label: 'Trash',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
			api: UmbMediaTrashEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
];

export const manifests = [...entityActions];
