import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_TREE_ALIAS, UMB_ELEMENT_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_DUPLICATE,
} from '../../user-permissions/constants.js';
import { UMB_DUPLICATE_ELEMENT_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityAction.Element.DuplicateTo',
		name: 'Duplicate Element To Entity Action',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_ELEMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_ELEMENT_TREE_ALIAS,
			treeRepositoryAlias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
			foldersOnly: true,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_DUPLICATE],
			},
			{ alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS },
		],
	},
	...repositoryManifests,
];
