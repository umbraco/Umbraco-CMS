import { UMB_MEMBER_GROUP_ENTITY_TYPE, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.MemberGroup.Create',
		name: 'Create Member Group Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
	...defaultManifests,
];
