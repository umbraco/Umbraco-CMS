import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../../collection/constants.js';
import {
	UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
} from '../../../constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_DELETE } from '../../../user-permissions/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND } from '@umbraco-cms/backoffice/relations';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND,
		alias: 'Umb.EntityBulkAction.Document.Trash',
		name: 'Trash Document Entity Bulk Action',
		weight: 10,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
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
