import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/manifests.js';
import { UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const bulkDuplicateAction: ManifestTypes = {
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
			alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
			match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkCopy,
		},
	],
};

export const manifests: Array<ManifestTypes> = [bulkDuplicateAction, ...repositoryManifests];
