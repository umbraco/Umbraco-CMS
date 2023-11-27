import { UMB_MEDIA_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbMediaMoveEntityBulkAction } from './move/move.action.js';
import { UmbMediaCopyEntityBulkAction } from './copy/copy.action.js';
import { UmbMediaTrashEntityBulkAction } from './trash/trash.action.js';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 100,
		api: UmbMediaMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: UMB_MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Copy',
		name: 'Copy Media Entity Bulk Action',
		weight: 90,
		api: UmbMediaCopyEntityBulkAction,
		meta: {
			label: 'Copy',
			repositoryAlias: UMB_MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Trash',
		name: 'Trash Media Entity Bulk Action',
		weight: 80,
		api: UmbMediaTrashEntityBulkAction,
		meta: {
			label: 'Trash',
			repositoryAlias: UMB_MEDIA_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...entityActions];
