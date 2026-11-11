import { UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MemberGroup.Delete',
		name: 'Delete Member Group Entity Action',
		forEntityTypes: [UMB_MEMBER_GROUP_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
				operation: 'delete',
			},
		],
	},
	...createManifests,
];
