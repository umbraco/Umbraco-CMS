import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/manifests.js';
import { manifests as createManifests } from './create/manifests.js';
import { UMB_MEMBER_REFERENCE_REPOSITORY_ALIAS } from '../reference/constants.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action',
		kind: 'deleteWithRelation',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_MEMBER_REFERENCE_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
];