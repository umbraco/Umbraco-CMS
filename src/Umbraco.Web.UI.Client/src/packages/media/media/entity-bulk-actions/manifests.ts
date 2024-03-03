import type { UmbCollectionBulkActionPermissions } from '../../../core/collection/types.js';
import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbMediaMoveEntityBulkAction } from './move/move.action.js';
import { UmbDuplicateMediaEntityBulkAction } from './copy/copy.action.js';
import { UmbMediaDeleteEntityBulkAction } from './delete/delete.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Duplicate',
		name: 'Duplicate Media Entity Bulk Action',
		weight: 30,
		api: UmbDuplicateMediaEntityBulkAction,
		meta: {
			label: 'Duplicate',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkCopy,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 20,
		api: UmbMediaMoveEntityBulkAction,
		meta: {
			label: 'Move',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkMove,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Delete',
		name: 'Delete Media Entity Bulk Action',
		weight: 10,
		api: UmbMediaDeleteEntityBulkAction,
		meta: {
			label: 'Delete',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkDelete,
			},
		],
	},
];
