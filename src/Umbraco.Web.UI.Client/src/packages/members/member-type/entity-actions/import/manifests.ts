import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberType.Import',
		name: 'Export Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
		api: () => import('./member-type-import.action.js'),
		meta: {
			icon: 'icon-page-up',
			label: '#actions_import',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
	...repositoryManifests,
	...modalManifests,
];
