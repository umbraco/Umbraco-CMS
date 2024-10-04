import { UMB_MEDIA_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TREE_ALIAS } from '../../tree/constants.js';
import { UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
} from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';

const bulkMoveAction: UmbExtensionManifest = {
	type: 'entityBulkAction',
	kind: 'moveTo',
	alias: 'Umb.EntityBulkAction.Media.MoveTo',
	name: 'Move Media Entity Bulk Action',
	weight: 20,
	forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	meta: {
		bulkMoveRepositoryAlias: UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS,
		treeAlias: UMB_MEDIA_TREE_ALIAS,
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_COLLECTION_ALIAS,
		},
		{
			alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
			match: (permissions: UmbCollectionBulkActionPermissions) => permissions.allowBulkMove,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [bulkMoveAction, ...repositoryManifests];
