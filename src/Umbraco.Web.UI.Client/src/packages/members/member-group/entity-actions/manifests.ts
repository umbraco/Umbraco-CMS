import { UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberGroup.Create',
		name: 'Create Member Group Entity Action',
		weight: 1200,
		api: () => import('./create-member-group.action.js'),
		forEntityTypes: [UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MemberGroup.Delete',
		name: 'Delete Member Group Entity Action ',
		forEntityTypes: [UMB_MEMBER_GROUP_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
		},
	},
];
