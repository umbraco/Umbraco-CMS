import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		forEntityTypes: [UMB_LANGUAGE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
];
