import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action.js';
import { UmbDocumentDeleteEntityBulkAction } from './delete/delete.action.js';
import { UmbDocumentMoveEntityBulkAction } from './move/move.action.js';
import { UmbDocumentPublishEntityBulkAction } from './publish/publish.action.js';
import { UmbDocumentUnpublishEntityBulkAction } from './unpublish/unpublish.action.js';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

// TODO: [LK] Wondering how these actions could be wired up to the `bulkActionPermissions` config?
export const manifests: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Publish',
		name: 'Publish Document Entity Bulk Action',
		weight: 50,
		api: UmbDocumentPublishEntityBulkAction,
		meta: {
			label: 'Publish',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Unpublish',
		name: 'Unpublish Document Entity Bulk Action',
		weight: 40,
		api: UmbDocumentUnpublishEntityBulkAction,
		meta: {
			label: 'Unpublish',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Copy',
		name: 'Copy Document Entity Bulk Action',
		weight: 30,
		api: UmbDocumentCopyEntityBulkAction,
		meta: {
			label: 'Copy',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 20,
		api: UmbDocumentMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Delete',
		name: 'Delete Document Entity Bulk Action',
		weight: 10,
		api: UmbDocumentDeleteEntityBulkAction,
		meta: {
			label: 'Delete',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
];
