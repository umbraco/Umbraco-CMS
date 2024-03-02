import {
	UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
	UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
} from '../repository/index.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberGroup.Delete',
		name: 'Delete Member Group Entity Action ',
		kind: 'delete',
		meta: {
			detailRepositoryAlias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEMBER_GROUP_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
