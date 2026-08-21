import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import { UMB_DICTIONARY_TREE_ALIAS, UMB_DICTIONARY_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.Dictionary.MoveTo',
		name: 'Move Dictionary Entity Action',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS,
			treeAlias: UMB_DICTIONARY_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_DICTIONARY_ENTITY_TYPE,
				operation: 'update',
			},
		],
	},
	...repositoryManifests,
];
