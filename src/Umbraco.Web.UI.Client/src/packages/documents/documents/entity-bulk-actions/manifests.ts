import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDuplicateEntityBulkAction } from './duplicate/duplicate.action.js';
import { UmbDocumentDeleteEntityBulkAction } from './delete/delete.action.js';
import { UmbMoveDocumentEntityBulkAction } from './move/move.action.js';
import { UmbDocumentPublishEntityBulkAction } from './publish/publish.action.js';
import { UmbDocumentUnpublishEntityBulkAction } from './unpublish/unpublish.action.js';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';

export const manifests: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Publish',
		name: 'Publish Document Entity Bulk Action',
		weight: 50,
		api: UmbDocumentPublishEntityBulkAction,
		meta: {
			label: 'Publish',
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
		api: UmbDocumentUnpublishEntityBulkAction,
		meta: {
			label: 'Unpublish',
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
	/* TODO: implement bulk move action
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.MoveTo',
		name: 'Move Document Entity Bulk Action',
		weight: 20,
		api: UmbMoveDocumentEntityBulkAction,
		meta: {
			label: 'Move',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
				match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkMove,
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
