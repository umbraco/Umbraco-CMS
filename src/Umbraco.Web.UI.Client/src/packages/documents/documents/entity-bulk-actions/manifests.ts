import { UMB_DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbDocumentMoveEntityBulkAction } from './move/move.action.js';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action.js';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 10,
		api: UmbDocumentMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: UMB_DOCUMENT_REPOSITORY_ALIAS,
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
			repositoryAlias: UMB_DOCUMENT_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				// TODO: this condition should be based on entity types in the selection
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_ENTITY_TYPE,
			},
		],
	},
];

export const manifests = [...entityActions];
