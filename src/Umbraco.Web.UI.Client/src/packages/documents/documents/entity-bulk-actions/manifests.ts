import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbDocumentMoveEntityBulkAction } from './move/move.action.js';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action.js';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'document';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 10,
		api: UmbDocumentMoveEntityBulkAction,
		meta: {
			label: 'Move',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Copy',
		name: 'Copy Document Entity Bulk Action',
		weight: 9,
		api: UmbDocumentCopyEntityBulkAction,
		meta: {
			label: 'Copy',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
];

export const manifests = [...entityActions];
