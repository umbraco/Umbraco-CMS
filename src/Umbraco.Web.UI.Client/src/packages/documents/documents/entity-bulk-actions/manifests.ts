import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
//import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/manifests.js';
import { manifests as moveToManifests } from './move-to/manifests.js';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityBulkAction, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';

export const entityBulkActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Publish',
		name: 'Publish Document Entity Bulk Action',
		weight: 50,
		api: () => import('./publish/publish.action.js'),
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkPublish,
			},
		],
	},
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Unpublish',
		name: 'Unpublish Document Entity Bulk Action',
		weight: 40,
		api: () => import('./unpublish/unpublish.action.js'),
		meta: {
			icon: 'icon-globe',
			label: '#actions_unpublish',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkUnpublish,
			},
		],
	},
	/* TODO: implement bulk duplicate action
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Duplicate',
		name: 'Duplicate Document Entity Bulk Action',
		weight: 30,
		api: UmbDocumentDuplicateEntityBulkAction,
		meta: {
			label: 'Duplicate...',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkCopy,
			},
		],
	},
	*/
	/* TODO: implement bulk trash action
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Delete',
		name: 'Delete Document Entity Bulk Action',
		weight: 10,
		api: UmbDocumentDeleteEntityBulkAction,
		meta: {
			label: 'Delete',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkDelete,
			},
		],
	},
	*/
];

export const manifests: Array<ManifestTypes> = [...entityBulkActions, ...moveToManifests];
