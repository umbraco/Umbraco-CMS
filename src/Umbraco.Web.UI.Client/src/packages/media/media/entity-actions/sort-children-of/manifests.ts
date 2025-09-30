import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TREE_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	{
		type: 'entityAction',
		kind: 'sortChildrenOfContent',
		alias: 'Umb.EntityAction.Media.SortChildrenOf',
		name: 'Sort Children Of Media Entity Action',
		forEntityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE, UMB_MEDIA_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
			sortChildrenOfRepositoryAlias: UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS,
			treeRepositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
