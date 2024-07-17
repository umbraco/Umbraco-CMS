import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { manifests as moveToManifests } from './move-to/manifests.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityBulkAction, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';

const entityBulkActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Duplicate',
		name: 'Duplicate Media Entity Bulk Action',
		weight: 30,
		api: () => import('./duplicate/duplicate.action.js'),
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
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
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
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
