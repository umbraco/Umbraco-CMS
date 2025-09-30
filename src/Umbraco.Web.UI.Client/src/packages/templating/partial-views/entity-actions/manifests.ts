import { UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS, UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete Partial View Entity Action',
		forEntityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
	...renameManifests,
];
