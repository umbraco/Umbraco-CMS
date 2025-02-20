import { UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbCreateMemberTypeEntityAction } from './create.action.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberType.Create',
		name: 'Create Member Type Entity Action',
		weight: 1200,
		api: UmbCreateMemberTypeEntityAction,
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MemberType.Delete',
		name: 'Delete Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
		},
	},
	...duplicateManifests,
];
