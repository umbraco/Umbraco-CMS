import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.MemberType.MoveTo',
		name: 'Move Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
			foldersOnly: true,
		},
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
				operation: 'update',
			},
		],
	},
	...repositoryManifests,
];
