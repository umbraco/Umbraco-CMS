import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { manifests as moveToManifests } from './move-to/manifests.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';

const entityBulkActions: Array<ManifestTypes> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Duplicate',
		name: 'Duplicate Media Entity Bulk Action',
		weight: 30,
		api: () => import('./duplicate/duplicate.action.js'),
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
		alias: 'Umb.EntityBulkAction.Media.Delete',
		name: 'Delete Media Entity Bulk Action',
		weight: 10,
		api: () => import('./delete/delete.action.js'),
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

export const manifests: Array<ManifestTypes> = [...entityBulkActions, ...moveToManifests];
