import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../../../entity.js';
import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_TREE_ALIAS } from '../../../../constants.js';
import { UMB_MOVE_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.MemberType.Folder.MoveTo',
		name: 'Move Member Type Folder Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
