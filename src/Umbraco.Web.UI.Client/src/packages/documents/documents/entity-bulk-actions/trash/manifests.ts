import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';

const bulkTrashAction: ManifestTypes = {
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
			alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
			match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkDelete,
		},
	],
};

export const manifests: Array<ManifestTypes> = [bulkTrashAction, ...repositoryManifests];
