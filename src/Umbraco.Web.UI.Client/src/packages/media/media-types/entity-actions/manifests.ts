import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
	UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
} from '../constants.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as exportManifests } from './export/manifests.js';
import { manifests as importManifests } from './import/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
	...moveManifests,
	...duplicateManifests,
	...exportManifests,
	...importManifests,
];
