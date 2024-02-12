import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbDocumentMoveEntityBulkAction } from './move/move.action.js';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action.js';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const entityActions: Array<ManifestEntityBulkAction> = [

	// TODO: [LK] Add bulk entity actions for Publish, Unpublish and Delete.
	// TODO: [LK] Wondering how these actions could be wired up to the `bulkActionPermissions` config?

	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 10,
		api: UmbDocumentMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Copy',
		name: 'Copy Document Entity Bulk Action',
		weight: 9,
		api: UmbDocumentCopyEntityBulkAction,
		meta: {
			label: 'Copy',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...entityActions];
