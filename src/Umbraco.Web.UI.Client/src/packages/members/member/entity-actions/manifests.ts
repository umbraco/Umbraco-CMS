import { UMB_MEMBER_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action',
		kind: 'delete',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions];
