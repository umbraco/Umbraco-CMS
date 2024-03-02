import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/index.js';
import { UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UmbCreateMemberTypeEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberType.Create',
		name: 'Create Member Type Entity Action',
		weight: 1000,
		api: UmbCreateMemberTypeEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create...',
			repositoryAlias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberType.Delete',
		name: 'Delete Member Type Entity Action',
		kind: 'delete',
		meta: {
			detailRepositoryAlias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
