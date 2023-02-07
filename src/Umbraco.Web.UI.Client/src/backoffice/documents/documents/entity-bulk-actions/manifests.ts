import { UmbDocumentMoveEntityBulkAction } from './move/move.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/extensions-registry';

const entityType = 'document';
const repositoryAlias = 'Umb.Repository.Documents';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 1000,
		meta: {
			entityType,
			label: 'Move',
			repositoryAlias,
			api: UmbDocumentMoveEntityBulkAction,
		},
	},
];

export const manifests = [...entityActions];
