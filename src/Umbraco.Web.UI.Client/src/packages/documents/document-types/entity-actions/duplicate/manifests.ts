import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_TREE_ALIAS, UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityAction.DocumentType.DuplicateTo',
		name: 'Duplicate Document To Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
			treeRepositoryAlias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
