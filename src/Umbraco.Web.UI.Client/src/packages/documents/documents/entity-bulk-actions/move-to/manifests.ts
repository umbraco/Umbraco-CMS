import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_MOVE } from '../../user-permissions/document/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.MoveTo',
		name: 'Move Document Entity Bulk Action',
		weight: 20,
		api: () => import('./move-document-bulk.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			label: '#actions_move',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_MOVE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	...repositoryManifests,
];
