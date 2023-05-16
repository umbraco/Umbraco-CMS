import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbDocumentMoveEntityBulkAction } from './move/move.action';
import { UmbDocumentCopyEntityBulkAction } from './copy/copy.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'document';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Move',
		name: 'Move Document Entity Bulk Action',
		weight: 10,
		meta: {
			label: 'Move',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentMoveEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Document.Copy',
		name: 'Copy Document Entity Bulk Action',
		weight: 9,
		meta: {
			label: 'Copy',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentCopyEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
];

export const manifests = [...entityActions];
