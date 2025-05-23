import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TREE_REPOSITORY_ALIAS, UMB_MEDIA_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.Media.MoveTo',
		name: 'Move Media Entity Action',
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_MEDIA_REPOSITORY_ALIAS,
			treeAlias: UMB_MEDIA_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	...repositoryManifests,
];
