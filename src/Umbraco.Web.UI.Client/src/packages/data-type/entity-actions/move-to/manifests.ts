import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS, UMB_DATA_TYPE_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.DataType.MoveTo',
		name: 'Move Data Type Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_DATA_TYPE_TREE_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
