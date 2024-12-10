import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/manifests.js';
import { UMB_USER_PERMISSION_DOCUMENT_MOVE } from '../../user-permissions/constants.js';
import { UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'moveTo',
		alias: 'Umb.EntityBulkAction.Document.MoveTo',
		name: 'Move Document Entity Bulk Action',
		weight: 20,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			bulkMoveRepositoryAlias: UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
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
		],
	},
	...repositoryManifests,
];
