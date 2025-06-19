import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.MediaType.MoveTo',
		name: 'Move Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_MEDIA_TYPE_TREE_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
