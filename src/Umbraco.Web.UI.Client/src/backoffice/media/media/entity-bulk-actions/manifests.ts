import { UmbMediaMoveEntityBulkAction } from './move/move.action';
import { UmbMediaCopyEntityBulkAction } from './copy/copy.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/extensions-registry';

const entityType = 'media';
const repositoryAlias = 'Umb.Repository.Media';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Move',
		name: 'Move Media Entity Bulk Action',
		weight: 100,
		meta: {
			entityType,
			label: 'Move',
			repositoryAlias,
			api: UmbMediaMoveEntityBulkAction,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.Media.Copy',
		name: 'Copy Media Entity Bulk Action',
		weight: 90,
		meta: {
			entityType,
			label: 'Copy',
			repositoryAlias,
			api: UmbMediaCopyEntityBulkAction,
		},
	},
];

export const manifests = [...entityActions];
