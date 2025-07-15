import { UMB_MEDIA_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../../repository/constants.js';
import { UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from '../../repository/constants.js';
import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from '../../../reference/constants.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../../../collection/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND } from '@umbraco-cms/backoffice/relations';

const bulkTrashAction: UmbExtensionManifest = {
	type: 'entityBulkAction',
	kind: UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND,
	alias: 'Umb.EntityBulkAction.Media.Trash',
	name: 'Trash Media Entity Bulk Action',
	weight: 10,
	forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	meta: {
		itemRepositoryAlias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
		recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
		referenceRepositoryAlias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_COLLECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [bulkTrashAction, ...repositoryManifests];
