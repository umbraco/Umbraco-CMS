import { UMB_MEDIA_COLLECTION_ALIAS, UMB_MEDIA_TREE_ALIAS } from '../../constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

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
	],
};

export const manifests: Array<UmbExtensionManifest> = [bulkMoveAction, ...repositoryManifests];
