import { UMB_MEDIA_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const bulkTrashAction: UmbExtensionManifest = {
	type: 'entityBulkAction',
	kind: 'trash',
	alias: 'Umb.EntityBulkAction.Media.Trash',
	name: 'Trash Media Entity Bulk Action',
	weight: 10,
	forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	meta: {
		bulkTrashRepositoryAlias: UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS,
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_COLLECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [bulkTrashAction, ...repositoryManifests];
