import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_TREE_ALIAS } from '../../tree/index.js';
import { UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
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
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...repositoryManifests];
