import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS, UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../../../index.js';
import { UMB_MOVE_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.DocumentBlueprint.Folder.MoveTo',
		name: 'Move Document Blueprint Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
