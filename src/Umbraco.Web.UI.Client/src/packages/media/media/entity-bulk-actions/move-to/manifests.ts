import { UMB_MEDIA_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const bulkMoveAction: UmbExtensionManifest = {
	type: 'entityBulkAction',
	kind: 'default',
	alias: 'Umb.EntityBulkAction.Media.MoveTo',
	name: 'Move Media Entity Bulk Action',
	weight: 20,
	api: () => import('./move-media-bulk.action.js'),
	forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	meta: {
		label: '#actions_move',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_MEDIA_COLLECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [bulkMoveAction, ...repositoryManifests];
