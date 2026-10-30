import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Member Group Collection Action',
		alias: 'Umb.CollectionAction.MemberGroup.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: 'section/member-management/workspace/member-group/create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.MemberGroup',
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
];
