import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
} from '../index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DocumentBlueprintItem.Delete',
		name: 'Delete Document Blueprint Item Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
	...moveManifests,
];
