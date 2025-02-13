import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/manifests.js';
import { manifests as createManifests } from './create/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
];
