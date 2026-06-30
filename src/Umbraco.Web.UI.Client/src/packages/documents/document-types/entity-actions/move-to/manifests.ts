import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.DocumentType.MoveTo',
		name: 'Move Document Type Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
