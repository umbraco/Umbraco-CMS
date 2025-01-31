import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_DELETE } from '../../user-permissions/constants.js';
import { UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'trash',
		alias: 'Umb.EntityBulkAction.Document.Trash',
		name: 'Trash Document Entity Bulk Action',
		weight: 10,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			bulkTrashRepositoryAlias: UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
			},
		],
	},
	...repositoryManifests,
];
