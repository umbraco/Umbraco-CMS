import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/manifests.js';
import { UMB_USER_PERMISSION_DOCUMENT_DUPLICATE } from '../../user-permissions/constants.js';
import { UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityBulkAction.Document.DuplicateTo',
		name: 'Duplicate Document Entity Bulk Action',
		weight: 30,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			bulkDuplicateRepositoryAlias: UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DUPLICATE],
			},
		],
	},
	...repositoryManifests,
];
