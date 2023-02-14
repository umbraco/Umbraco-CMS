import { UmbDocumentMoveEntityBulkAction } from './move/move.action';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/extensions-registry';

const entityType = 'document';
const repositoryAlias = 'Umb.Repository.Documents';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 10,
		meta: {
			entityType,
			label: 'Move',
			repositoryAlias,
			api: UmbDocumentMoveEntityBulkAction,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Copy',
		name: 'Copy Document Entity Bulk Action',
		weight: 9,
		meta: {
			entityType,
			label: 'Copy',
			repositoryAlias,
			api: UmbDocumentCopyEntityBulkAction,
		},
	},
];

export const manifests = [...entityActions];
