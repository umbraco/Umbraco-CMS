import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbMediaMoveEntityBulkAction } from './move/move.action.js';
import { UmbMediaCopyEntityBulkAction } from './copy/copy.action.js';
import { UmbMediaTrashEntityBulkAction } from './trash/trash.action.js';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'media';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 100,
		api: UmbMediaMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Copy',
		name: 'Copy Media Entity Bulk Action',
		weight: 90,
		api: UmbMediaCopyEntityBulkAction,
		meta: {
			label: 'Copy',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Trash',
		name: 'Trash Media Entity Bulk Action',
		weight: 80,
		api: UmbMediaTrashEntityBulkAction,
		meta: {
			label: 'Trash',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
];

export const manifests = [...entityActions];
