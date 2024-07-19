import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import { UMB_DICTIONARY_TREE_ALIAS, UMB_DICTIONARY_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
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
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...repositoryManifests];
